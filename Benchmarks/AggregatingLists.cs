using System;
using System.Collections.Generic;
using System.Linq;

namespace csharp_benchmarks.Benchmarks
{
  public class AggregatingLists
  {
    static int iterations = 1000;
    static Customer[] customers;
    static CustomerPreference[] customersPreferences;
    static List<CustomerAggregate> customerAggregates;
    static Dictionary<int, CustomerPreference> customersPreferencesDict;


    public static void Init(string[] args)
    {
      if (args.Length > 0) iterations = Int32.Parse(args[0]);
      customers = Enumerable.Range(0, iterations).Select((i) => new Customer { Id = i, Name = $"Name ${i}" }).ToArray();
      customersPreferences = Enumerable.Range(0, iterations).Select((i) => new CustomerPreference { CustomerId = i, Total = i }).ToArray();
      customersPreferencesDict = customersPreferences.ToDictionary(c => c.CustomerId);
    }

    public static void Reset()
    {
      customerAggregates = null;
    }

    public static void Check()
    {
      if (customerAggregates.Count != iterations)
        throw new Exception("List doesn't have the right size.");
    }

    [Benchmark]
    public static void With_a_For_Loop_And_Lookup()
    {
      customerAggregates = new List<CustomerAggregate>();
      for (var i = 0; i < customers.Length; i++)
      {
        var customer = customers[i];
        var preference = customersPreferences.SingleOrDefault(total => total.CustomerId == customer.Id);
        customerAggregates.Add(new CustomerAggregate
        {
          CustomerId = customer.Id,
          Name = customer.Name,
          Preference = preference
        });
      }
    }


    [Benchmark]
    public static void With_a_ForEach_Loop_And_Lookup()
    {
      customerAggregates = new List<CustomerAggregate>();
      foreach (var customer in customers)
      {
        var preference = customersPreferences.SingleOrDefault(total => total.CustomerId == customer.Id);
        customerAggregates.Add(new CustomerAggregate
        {
          CustomerId = customer.Id,
          Name = customer.Name,
          Preference = preference
        });
      }
    }

    [Benchmark]
    public static void With_A_Select_And_Lookup()
    {
      customerAggregates = customers
        .Select(customer =>
        {
          var preference = customersPreferences.SingleOrDefault(total => total.CustomerId == customer.Id);
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
    public static void With_A_Join()
    {
      customerAggregates = customers.Join(
        customersPreferences,
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
    public static void With_A_Query_Join()
    {
      customerAggregates = (from customer in customers
                            join preference in customersPreferences on customer.Id equals preference.CustomerId
                            select new CustomerAggregate
                            {
                              CustomerId = customer.Id,
                              Name = customer.Name,
                              Preference = preference
                            }).ToList();
    }


    [Benchmark]
    public static void With_A_Dict_Created()
    {
      var customersDict = customersPreferences.ToDictionary(k => k.CustomerId);
      customerAggregates = customers
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
    public static void With_A_Dict_OnTheFly()
    {
      customerAggregates = customers
        .Select(customer =>
        {
          var preference = customersPreferencesDict[customer.Id];
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
    public static void Manual_Iteration()
    {
      var preferences = new Dictionary<int, CustomerPreference>(customersPreferences.Length);
      foreach (var p in customersPreferences)
      {
        preferences.Add(p.CustomerId, p);
      }

      customerAggregates = new List<CustomerAggregate>(customers.Length);
      foreach (var customer in customers)
      {
        preferences.TryGetValue(customer.Id, out var preference);
        customerAggregates.Add(new CustomerAggregate
        {
          CustomerId = customer.Id,
          Name = customer.Name,
          Preference = preference,
        });
      }
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
