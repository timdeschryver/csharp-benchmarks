using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace WithBenchmarkDotNet
{
    [MemoryDiagnoser]
    [MarkdownExporter]
    public class JoinBenchmark
    {
        Customer[] _customers;
        CustomerPreference[] _customersPreferences;
        Dictionary<int, CustomerPreference> _customersPreferencesDict;

        [Params(1, 10, 100, 1_000, 10_000/*, 100_000, 200_000, 300_000, 400_000,500_000*/ )]
        public int ListSize;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _customers = Enumerable.Range(0, ListSize).Select((i) => new Customer { Id = i, Name = $"Name ${i}" }).ToArray();
            _customersPreferences = Enumerable.Range(0, ListSize).Select((i) => new CustomerPreference { CustomerId = i, Total = i }).ToArray();
            _customersPreferencesDict = _customersPreferences.ToDictionary(c => c.CustomerId);
        }

        [Benchmark]
        public List<CustomerAggregate> With_a_For_Loop_And_Lookup()
        {
            var customerAggregates = new List<CustomerAggregate>();
            for (var i = 0; i < _customers.Length; i++)
            {
                var customer = _customers[i];
                var preference = _customersPreferences.SingleOrDefault(total => total.CustomerId == customer.Id);
                customerAggregates.Add(new CustomerAggregate
                {
                    CustomerId = customer.Id,
                    Name = customer.Name,
                    Preference = preference
                });
            }
            return customerAggregates;
        }

        [Benchmark]
        public List<CustomerAggregate> With_a_ForEach_Loop_And_Lookup()
        {
            var customerAggregates = new List<CustomerAggregate>();
            foreach (var customer in _customers)
            {
                var preference = _customersPreferences.SingleOrDefault(total => total.CustomerId == customer.Id);
                customerAggregates.Add(new CustomerAggregate
                {
                    CustomerId = customer.Id,
                    Name = customer.Name,
                    Preference = preference
                });
            }
            return customerAggregates;
        }

        [Benchmark]
        public List<CustomerAggregate> With_A_Select_And_Lookup()
        {
            return _customers
              .Select(customer =>
              {
                  var preference = _customersPreferences.SingleOrDefault(total => total.CustomerId == customer.Id);
                  return new CustomerAggregate
                  {
                      CustomerId = customer.Id,
                      Name = customer.Name,
                      Preference = preference
                  };
              })
              .ToList();
        }

        [Benchmark]
        public List<CustomerAggregate> With_A_Join()
        {
            return _customers.Join(
              _customersPreferences,
              customer => customer.Id,
              preference => preference.CustomerId,
              (customer, preference) => new CustomerAggregate
              {
                  CustomerId = customer.Id,
                  Name = customer.Name,
                  Preference = preference
              })
              .ToList();
        }

        [Benchmark]
        public List<CustomerAggregate> With_A_Query_Join()
        {
            return (from customer in _customers
                    join preference in _customersPreferences on customer.Id equals preference.CustomerId
                    select new CustomerAggregate
                    {
                        CustomerId = customer.Id,
                        Name = customer.Name,
                        Preference = preference
                    }).ToList();
        }

        [Benchmark]
        public List<CustomerAggregate> With_A_Dict_Created()
        {
            var customersDict = _customersPreferences.ToDictionary(k => k.CustomerId);
            return _customers
              .Select(customer =>
              {
                  var preference = customersDict[customer.Id];
                  return new CustomerAggregate
                  {
                      CustomerId = customer.Id,
                      Name = customer.Name,
                      Preference = preference
                  };
              })
              .ToList();
        }

        [Benchmark]
        public List<CustomerAggregate> With_A_Dict_OnTheFly()
        {
            return _customers
              .Select(customer =>
              {
                  var preference = _customersPreferencesDict[customer.Id];
                  return new CustomerAggregate
                  {
                      CustomerId = customer.Id,
                      Name = customer.Name,
                      Preference = preference
                  };
              })
              .ToList();
        }

        [Benchmark]
        public List<CustomerAggregate> Manual_Iteration()
        {
            var preferences = new Dictionary<int, CustomerPreference>(_customersPreferences.Length);
            foreach (var p in _customersPreferences)
            {
                preferences.Add(p.CustomerId, p);
            }

            var customerAggregates = new List<CustomerAggregate>(_customers.Length);
            foreach (var customer in _customers)
            {
                preferences.TryGetValue(customer.Id, out var preference);
                customerAggregates.Add(new CustomerAggregate
                {
                    CustomerId = customer.Id,
                    Name = customer.Name,
                    Preference = preference,
                });
            }
            return customerAggregates;
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class CustomerPreference
        {
            public int CustomerId { get; set; }
            public int Total { get; set; }
        }

        public class CustomerAggregate
        {
            public int CustomerId { get; set; }
            public string Name { get; set; }
            public CustomerPreference Preference { get; set; }
        }
    }
}
