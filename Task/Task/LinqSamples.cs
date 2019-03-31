// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SampleSupport;
using Task.Data;

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 1")]
        [Description("Get a list of all clients whose total turnover exceeds some value of X.")]
        public void Linq001()
        {
            var customers = GetCustomersBySumTotalOrder(100);

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        private IEnumerable<Customer> GetCustomersBySumTotalOrder(decimal total)
        {
            if (total < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(total));
            }

            return dataSource.Customers
                .Where(c => c.Orders.Sum(o => o.Total) > total);
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 2")]
        [Description("For all clients get suppliers who are based in same country and in same city.")]
        public void Linq002()
        {
            var customers = dataSource.Customers
                .Join(dataSource.Suppliers, c => new { c.Country, c.City }, s => new { s.Country, s.City }, (c, s) => c);

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 2 Another variant")]
        [Description("For all clients get supppliers who are based in same country and in same city.")]
        public void Linq002Another()
        {
            var customers = dataSource.Customers.SelectMany(
                c => dataSource.Suppliers.Where(s => s.Country == c.Country && s.City == c.City),
                (c, s) => c);

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 3")]
        [Description("Find all clients who had orders with cost higher than X.")]
        public void Linq003()
        {
            var customers = GetCustomersByTotalOrder(10000);

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        private IEnumerable<Customer> GetCustomersByTotalOrder(decimal total)
        {
            if (total < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(total));
            }

            return dataSource.Customers.SelectMany(c => c.Orders.Where(o => o.Total > total), (c, o) => c)
                .GroupBy(c => c.CustomerID)
                .Select(group => group.First());
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 4")]
        [Description("Select clients with date of their first order.")]
        public void Linq004()
        {
            var customers = dataSource.Customers.Select(c => new
                {
                    CustomerId = c.CustomerID,
                    MinDate = (c.Orders.Length > 0)
                        ? c.Orders.Min(o => o.OrderDate)
                        : (DateTime?)null
                })
                .Select(c => new
                {
                    c.CustomerId,
                    MinYear = c.MinDate?.Year,
                    MinMonth = c.MinDate?.Month
                });

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 5")]
        [Description("Extra task 4.")]
        public void Linq005()
        {
            var customers = dataSource.Customers.Select(c => new
                {
                    Customer = c,
                    Total = c.Orders.Sum(o => o.Total),
                    MinDate = (c.Orders.Length > 0)
                        ? c.Orders.Min(o => o.OrderDate)
                        : (DateTime?) null
                })
                .Select(x => new
                {
                    x.Customer.CustomerID,
                    x.Customer.CompanyName,
                    x.Total,
                    MinYear = x.MinDate?.Year,
                    MinMonth = x.MinDate?.Month
                })
                .OrderBy(c => c.MinYear)
                .ThenBy(c => c.MinMonth)
                .ThenByDescending(c => c.Total)
                .ThenBy(c => c.CompanyName);

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 6")]
        [Description("Check users tax number.")]
        public void Linq006()
        {
            var postalCodeRegex = new Regex(@"[^\d]+");
            var phoneRegex = new Regex(@"^[^\(]{1}");

            var customers = dataSource.Customers.Where(c =>
                c.PostalCode == null
                || postalCodeRegex.IsMatch(c.PostalCode)
                || string.IsNullOrEmpty(c.Region)
                || c.Phone == null
                || phoneRegex.IsMatch(c.Phone)
            ).Select(c => new
            {
                c.CustomerID,
                c.PostalCode,
                c.Region,
                c.Phone
            });

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 7 Sorting")]
        [Description("Sorting implementation.")]
        public void Linq007_Sorting()
        {
            var products = dataSource.Products.OrderBy(product => product.Category)
                .ThenBy(product => product.UnitsInStock > 0)
                .ThenBy(product => product.UnitPrice);

            foreach (var product in products)
            {
                ObjectDumper.Write(product);
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 7 Grouping")]
        [Description("Grouping implementation.")]
        public void Linq007_Grouping()
        {
            var query = dataSource.Products.GroupBy(p => p.Category, (category, products) => new
            {
                Category = category,
                HasInStocksProducts = products.GroupBy(p => new
                {
                    HasInStocks = p.UnitsInStock == 0
                }, (hasInStocksGroup, hasInStocksGroupProducts) => new
                {
                    hasInStocksGroup.HasInStocks,
                    Products = hasInStocksGroupProducts.OrderBy(p => p.UnitPrice)
                })
            });

            foreach (var product in query)
            {
                Console.WriteLine();
                Console.WriteLine($@"Category: {product.Category}");

                foreach (var hasInStocksProduct in product.HasInStocksProducts)
                {
                    Console.WriteLine();
                    Console.WriteLine($@" # Has in stocks: {hasInStocksProduct.HasInStocks}");

                    foreach (var p in hasInStocksProduct.Products)
                    {
                        Console.WriteLine($@"Product id: {p.ProductID}, price: {p.UnitPrice}.");
                    }
                }
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 8 Sorting")]
        [Description("Sorting implementation.")]
        public void Linq008_Sorting()
        {
            var products = dataSource.Products.Select(p => new
            {
                p.ProductID,
                p.ProductName,
                PriceCategory = p.UnitPrice <= 10
                    ? "Cheap"
                    : (p.UnitPrice <= 20
                        ? "Medium"
                        : "Expensive")
            });

            foreach (var product in products)
            {
                ObjectDumper.Write(product);
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 8 Grouping")]
        [Description("Grouping implementation.")]
        public void Linq008_Grouping()
        {
            var query = dataSource.Products.GroupBy(p => new
            {
                Cheap = p.UnitPrice <= 10,
                Medium = p.UnitPrice > 10 && p.UnitPrice <= 20,
                Expensive = p.UnitPrice > 20
            }, (group, p) => new
            {
                Group = group,
                Products = p
            });

            foreach (var x in query)
            {
                if (x.Group.Cheap)
                {
                    Console.WriteLine("Cheap group:");
                }

                if (x.Group.Medium)
                {
                    Console.WriteLine("Medium group:");
                }

                if (x.Group.Expensive)
                {
                    Console.WriteLine("Expensive group:");
                }

                foreach (var product in x.Products)
                {
                    Console.WriteLine($@" - Product id: {product.ProductID}, price: {product.UnitPrice}");
                }
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 9")]
        [Description("Count average profit by city.")]
        public void Linq009()
        {
            var cities = dataSource.Customers
                .GroupBy(c => c.City, (city, customers) => new
                {
                    City = city,
                    AverageTotal = customers
                        .Select(c => c.Orders.Sum(o => o.Total)).Average(),
                    AverageIntensity = customers
                        .Select(c => c.Orders.Count()).Average()
                });

            foreach (var city in cities)
            {
                ObjectDumper.Write(city);
            }
        }

        [Category("LINQ Aliaksandr Liashkevich")]
        [Title("Task 10")]
        [Description("Statistics.")]
        public void Linq010()
        {
            WriteHeader("months");

            var months = Enumerable.Range(1, 12).ToArray();

            foreach (var month in months)
            {
                var customers = GetCustomersByFilter(o => o.OrderDate.Month == month);
                var averageOrdersCount = GetAverageOrdersCount(customers);

                WriteStatistic($@"Month: {month}", averageOrdersCount);
            }

            var years = dataSource.Customers.SelectMany(c => c.Orders)
                .Select(o => o.OrderDate.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToArray();

            WriteHeader("years");

            foreach (var year in years)
            {
                var customers = GetCustomersByFilter(o => o.OrderDate.Year == year);
                var averageOrdersCount = GetAverageOrdersCount(customers);

                WriteStatistic($@"Year: {year}", averageOrdersCount);
            }

            WriteHeader("years and months");

            foreach (var year in years)
            {
                foreach (var month in months)
                {
                    var customers = GetCustomersByFilter(o => o.OrderDate.Year == year && o.OrderDate.Month == month);
                    var averageOrdersCount = GetAverageOrdersCount(customers);

                    WriteStatistic($@"Year: {year}, month: {month}", averageOrdersCount);
                }
            }
        }

        private double GetAverageOrdersCount(IEnumerable<Statistic> customers)
        {
            return customers.Select(c => c.Orders.Count()).Average();
        }

        private IEnumerable<Statistic> GetCustomersByFilter(Func<Order, bool> orderPredicate)
        {
            if (orderPredicate == null)
            {
                throw new ArgumentNullException(nameof(orderPredicate));
            }

            return dataSource.Customers.Select(c => new Statistic
            {
                Customer = c,
                Orders = c.Orders.Where(orderPredicate).ToArray()
            });
        }

        private static void WriteStatistic(string info, double averageOrdersCount)
        {
            Console.WriteLine();
            Console.WriteLine(info);
            if (averageOrdersCount > 0)
            {
                Console.WriteLine($@"Average orders count by one customer: {averageOrdersCount}");
            }
            else
            {
                Console.WriteLine("There are no orders.");
            }
        }

        private static void WriteHeader(string title)
        {
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($@"Statistic by {title}.");
        }
    }
}
