using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApiExample.Filters;

public class QueryRequestExamplesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var extendedBody = operation.RequestBody.Content.ToDictionary(x => x.Key, x => x.Value);
        extendedBody["application/json"].Examples = new Dictionary<string, OpenApiExample>
        {
            {
                "SingleFilter", new()
                {
                    Summary = "Single Filter",
                    Description = "Here is an example of a single Equals filter on a single property.",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": \"Name\",\n      " +
                                              "\"operator\": \"Equals\",\n      \"value\": \"Sigurður\"\n    }\n  ]\n}"
                    ),
                }
            },
            {
                "MultipleFilters&&", new()
                {
                    Summary = "Multiple Filters with AND operator",
                    Description = "Here is an example with multiple filters on a single property with an AND operator. " +
                                  "In this case, both conditions need to be satisfied for an entry " +
                                  "to appear in the result (&& operator).",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": \"Name\",\n      " +
                                              "\"operator\": \"StartsWith\",\n      \"value\": \"B\"\n    },\n    {" +
                                              "\n      \"property\": \"Name\",\n      \"operator\": \"Contains\"," +
                                              "\n      \"value\": \"r\"\n    }\n  ]\n}"
                    ),
                }
            },
            {
                "MultipleFilters||", new()
                {
                    Summary = "Multiple Filters with OR operator",
                    Description = "Here is an example with multiple filters on a single property, joined with an OR " +
                                  "operator. In this case only one of the conditions need to be satisfied for an " +
                                  "entry to be included in the result (|| operator).",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": \"Name\",\n      " +
                                              "\"operator\": \"StartsWith\",\n      \"value\": \"B\"\n    },\n    " +
                                              "{\n      \"property\": \"Name\",\n      \"operator\": \"Contains\"," +
                                              "\n      \"value\": \"r\"\n    }\n  ],\n  \"filterExpression\": \"||\"\n}"
                    ),
                }
            },
            {
                "MultipleFiltersComplex", new()
                {
                    Summary = "Multiple Filters with Complex Logic",
                    Description = "Here is an advanced scenario in which we not only have multiple filters, but " +
                                  "also a complex logic for including entries. Specifically, we would like to get " +
                                  "goblins, whose name starts with \"B\", and are over 30-years-old, OR goblins, who " +
                                  "were born before July 11th 1996, and do not have bitter taste.",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": \"Name\",\n      " +
                                              "\"operator\": \"StartsWith\",\n      \"value\": \"B\"\n    },\n    {" +
                                              "\n      \"property\": \"Age\",\n      \"operator\": \"GreaterThan\"," +
                                              "\n      \"value\": \"30\"\n    },\n    {\n      \"property\": " +
                                              "\"DateOfBirth\",\n      \"operator\": \"LessThanOrEqual\",\n      " +
                                              "\"value\": \"1996-07-11 21:42:09.000\"\n    },\n    {\n      " +
                                              "\"property\": \"Taste\",\n      \"operator\": \"DoesNotEqual\",\n      " +
                                              "\"value\": \"Bitter\"\n    }\n  ],\n  \"filterExpression\": " +
                                              "\"(0 && 1) || (2 && 3)\"\n}"
                    ),
                }
            },
            {
                "MultipleFilters!", new()
                {
                    Summary = "Multiple Filters with NOT operator",
                    Description = "In addition to the AND (&&) and OR (||) operators, filter expressions also " +
                                  "support the NOT (!) operator. The \"!\" operator allows you to reverse the " +
                                  "condition of a filter without changing the filter operator. As you can see in " +
                                  "this example, we filter for \"IsActive=true\", but reverse it with ! in the " +
                                  "expression.",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": \"isactive\",\n      " +
                                              "\"operator\": \"Equals\",\n      \"value\": \"true\"\n    },\n    {" +
                                              "\n      \"property\": \"strength\",\n      \"operator\": " +
                                              "\"GreaterThan\",\n      \"value\": \"5.1\"\n    }\n  ],\n  " +
                                              "\"filterExpression\": \"!0 && 1\"\n}"
                    ),
                }
            },
            {
                "NestedAndNullable", new()
                {
                    Summary = "Filters on Nested and Nullable Properties",
                    Description = "MagiQuery supports both nested and nullable properites.\n\n" +
                                  "Nested properties are separated from their parents with dots.\n\n " +
                                  "If you want to do a null evaluation on a property, you just need to specify the " +
                                  "operator and omit the filter's \"value\" property in your request payload.",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": " +
                                              "\"Contract.Details.SigningTime\",\n      \"operator\": \"GreaterThan\"" +
                                              ",\n\t  \"value\": \"00:15:00\"\n    },\n    {\n      \"property\": " +
                                              "\"Contract\",\n      \"operator\": \"DoesNotEqual\"\n    },\n    {" +
                                              "\n      \"property\": \"Contract.Details.DaysOfEffect\",\n      " +
                                              "\"operator\": \"Equals\"\n    }\n  ]\n}"
                    ),
                }
            },
            {
                "Search", new()
                {
                    Summary = "Search on three properties",
                    Description = "Search is actually a variation of filtering. Just specify the properties you want " +
                                  "to search on as filters with the \"Contains\" operator and use the \"||\" filter " +
                                  "expression.",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": \"Name\",\n      " +
                                              "\"operator\": \"Contains\",\n      \"value\": \"ee\"\n    },\n    {" +
                                              "\n      \"property\": \"FavouriteLetter\",\n      \"operator\": " +
                                              "\"Contains\",\n      \"value\": \"ee\"\n    },\n    {\n      " +
                                              "\"property\": \"Taste\",\n      \"operator\": \"Contains\",\n      " +
                                              "\"value\": \"ee\"\n    }\n  ],\n  \"filterExpression\": \"||\"\n}"
                    ),
                }
            },
            {
                "SortSingle", new()
                {
                    Summary = "Single Sort",
                    Description = "Here is an example of how you can sort entries by a property. The default order" +
                                  " is ascending, unless specified otherwise with \"descending\":\"true\"",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": \"Name\",\n      " +
                                              "\"operator\": \"Contains\",\n      \"value\": \"e\"\n    }\n  ],\n  " +
                                              "\"sorts\":[\n   {\n     \"property\":\"FavouriteLetter\"\n   }\n  ]\n}"
                    ),
                }
            },
            {
                "MultipleSorts", new()
                {
                    Summary = "Multiple Sorts",
                    Description = "Here is an example of how you can sort entries by multiple properties. In this " +
                                  "case, we are first sorting by date of birth in ascending order, and then by " +
                                  "intelligence level in descending order.",
                    Value = new OpenApiString("{\n  \"filters\": [\n    {\n      \"property\": \"Salary\",\n      " +
                                              "\"operator\": \"GreaterThanOrEqual\",\n      \"value\": \"54001\"\n    }" +
                                              "\n  ],\n  \"sorts\":[\n   {\n     \"property\": \"DateOfBirth\"\n   }," +
                                              "\n   {\n     \"property\": \"IntelligenceLevel\",\n     " +
                                              "\"descending\": true\n   }\n  ]\n}"
                    ),
                }
            }
        };
        operation.RequestBody.Content = extendedBody;
    }
}