[![Mustackable](https://avatars.githubusercontent.com/u/200509271?s=96&v=4)](https://mustackable.dev)

<!-- TOC -->
  * [Intro](#intro)
  * [Quick Start](#quick-start)
  * [Query Request](#query-request)
  * [Property Types, Operators and Data Providers](#property-types-operators-and-data-providers)
    * [Supported Property Types](#supported-property-types)
    * [Supported Operators](#supported-operators)
    * [Supported Data Providers](#supported-data-providers)
    * [Nested and Nullable Properties](#nested-and-nullable-properties)
  * [Custom Filter Expressions](#custom-filter-expressions)
      * [Shorthand Expressions](#shorthand-expressions)
  * [Configuring Query Options](#configuring-query-options)
    * [Excluding Properties](#excluding-properties)
    * [Including ONLY Allowed Properties](#including-only-allowed-properties)
    * [Property Name Mapping](#property-name-mapping)
    * [Hiding Mapped Properties](#hiding-mapped-properties)
    * [Override DateTimeKind](#override-datetimekind)
    * [Specifying `BindingFlags` for Property Matching](#specifying-bindingflags-for-property-matching)
    * [Configuring `StringComparison` per Query](#configuring-stringcomparison-per-query)
  * [Override CultureInfo and Exact Parse Format](#override-cultureinfo-and-exact-parse-format)
    * [Extended Capabilities for `Runtime` IQueryables](#extended-capabilities-for-runtime-iqueryables)
  * [Examples](#examples)
    * [Single Filter](#single-filter)
    * [Multiple Filters with AND operator](#multiple-filters-with-and-operator)
    * [Multiple Filters with OR operator](#multiple-filters-with-or-operator)
    * [Multiple Filters with Complex Logic](#multiple-filters-with-complex-logic)
    * [Multiple Filters with NOT operator](#multiple-filters-with-not-operator)
    * [Filters on Nested and Nullable Properties](#filters-on-nested-and-nullable-properties)
    * [Search on Multiple Properties](#search-on-multiple-properties)
    * [Single Sort](#single-sort)
    * [Multiple Sorts](#multiple-sorts)
  * [License](#license)
<!-- TOC -->

## Intro

MagiQuery provides a unified API for generic filtering and sorting on C# IQueryables (such as EF entities).

The primary motivation for its development is assisting backend developers in creating simple, yet flexible data query endpoints.

With MagiQuery you can cover a wide range of requests - from the simplest queries to the most complex ones - with a single endpoint!

The idea is simple - you write it once, and it should be able to handle any scenario that might come up on the frontend.

## Quick Start

1. Add the NuGet package to your project:


    ```dotnet add package MagiQuery```

2. Get a reference to an IQueryable (be it from a DbContext or somewhere else) and use the ```ApplyQuery``` extension method like this:

    ```csharp
    [Route("Goblins")]
    public class QueryController(TestDbContext context) : ControllerBase
    {
        [HttpPost("Query")]
        [ProducesResponseType<IEnumerable<Goblin>>(StatusCodes.Status200OK)]
        public IActionResult QueryGoblins([FromBody] QueryRequest request)
        {
            var goblinsQuery = context.Goblins.ApplyQuery(request);
            return Ok(goblinsQuery.ToArray());
        }
    }
    ```

    ... or simply use the utility extension method ```GetPagedResponse``` to return a result with pagination like this:

    ```csharp
    [Route("Goblins")]
    public class QueryController(TestDbContext context) : ControllerBase
    {
        [HttpPost("Query")]
        [ProducesResponseType<QueryPagedResponse<Goblin>>(StatusCodes.Status200OK)]
        public IActionResult QueryGoblins([FromBody] QueryPagedRequest request)
            => Ok(context.Goblins.GetPagedResponse(request));
    }
    ```

3. Optionally, add a `JsonStringEnumConverter` to your serialization options, so you can refer to members of the `FilterOperator` enum by name, rather than by their `int` equivalents:

    ```csharp
    builder.Services
        .AddControllers()
            .AddJsonOptions(
                x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    ```

    If you prefer using the `int` equivalents for members of `FilterOperator`, please consult the table [here](#supported-operators).


4. Finally, hit the `/Goblins/Query` POST endpoint with a payload like this one:

    ```json5
    {
      "filters": [
        {
          "property": "Name", // Name of the property in the Goblin class
          "operator": "StartsWith", // The operator to use for filtering
          "value": "Wiz" // The value to filter with
        },
        {
          "property": "DateOfBirth",
          "operator": "GreaterThan",
          "value": "1010-10-01"
        }
      ]
    }
    ```

    ... and you will get a `Goblin` collection that consist only of goblins, whose names start with `Wiz`, and who were born after October 1st, 1010.  


We have provided a testing playground project [WebApiExample](https://github.com/mustackable-dev/MagiQuery/tree/main/example) in this repository, where you can experiment with MagiQuery and get a feel for how it works and what it can do for you.

It is preloaded with test data and uses an in-memory SQLite database. It has a Swagger implementation loaded with several examples of `QueryRequest` payloads.

## Query Request

The ```QueryRequest``` class configures the filtering and sorting parameters of a query on a given `IQueryable<TItem>`. It has two main components:

- **Filters** - a collection of filters that will be applied to the `IQueryable<TItem>`. A filter is defined with:
  - the name of a **property** of the class `TItem`
  - a string representation of the **value** you want to filter with on the specified property
  - a comparison **operator** from the predefined [`FilterOperator`](#supported-operators) enum

- **Sorts** - a collection of sort instructions to be applied to `IQueryable<TItem>`. A sort is defined with:
    - the name of a **property** of the class `TItem`
    - an optional boolean value for using **descending** order. If this property is omitted from the payload, MagiQuery automatically uses ascending order

Additionally, `QueryRequest` has two optional properties - [`FilterExpression`](#custom-filter-expressions) and [`OverrideCulture`](#override-cultureinfo-and-exact-parse-format).

MagiQuery also offers a utility class named `QueryRequestPaged`, which inherits from `QueryRequest` and has two extra properties for paging support.

The detailed structure of the two classes can be seen [here](https://github.com/mustackable-dev/MagiQuery/tree/main/src/MagiQuery/Models).

Examples of `QueryRequest` payloads can be seen [here](#examples).

## Property Types, Operators and Data Providers

### Supported Property Types

The following property types are supported in MagiQuery:

- `string`
- `char`
- `sbyte`
- `short`
- `ushort`
- `byte`
- `int`
- `uint`
- `long`
- `ulong`
- `float`
- `double`
- `decimal`
- `bool`
- `Enum`
- `DateTime`
- `DateTimeOffset`
- `DateOnly`
- `TimeSpan`
- `TimeOnly`

### Supported Operators

The following operators are supported in MagiQuery:

| Operator Name          | Operator `int` Id | Description                                                                                                                                                                                                                                     |
|------------------------|-------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Equals**             | 0                 | Relevant for all property types, including nullables.                                                                                                                                                                                           |
| **DoesNotEqual**       | 1                 | Relevant for all property types, including nullables.                                                                                                                                                                                           |
| **GreaterThan**        | 2                 | Relevant for all property types, except `string`, `char` and `bool`.                                                                                                                                                                            |
| **GreaterThanOrEqual** | 3                 | Relevant for all property types, except `string`, `char` and `bool`.                                                                                                                                                                            |
| **LessThan**           | 4                 | Relevant for all property types, except `string`, `char` and `bool`.                                                                                                                                                                            |
| **LessThanOrEqual**    | 5                 | Relevant for all property types, except `string`, `char` and `bool`.                                                                                                                                                                            |
| **StartsWith**         | 6                 | Works natively for properties of type `string`. For other types, MagiQuery will attempt to convert the property value to `string` during the filtering operation.                                                                               |
| **EndsWith**           | 7                 | Works natively for properties of type `string`. For other types, MagiQuery will attempt to convert the property value to `string` during the filtering operation.                                                                               |
| **Contains**           | 8                 | Works natively for properties of type `string`. For other types, MagiQuery will attempt to convert the property value to `string` during the filtering operation.                                                                               |
| **DoesNotContain**     | 9                 | Works natively for properties of type `string`. For other types, MagiQuery will attempt to convert the property value to `string` during the filtering operation.                                                                               |
| **IsEmpty**            | 10                | Applicable only to types `string` or `char`. Works exactly as `String.IsNullOrEmpty`.                                                                                                                                                           |
| **IsNotEmpty**         | 11                | Applicable only to types `string` or `char`. Works as an inverse of `String.IsNullOrEmpty`.                                                                                                                                                     |
| **Regex**              | 12                | Supported only with certain providers, see [here](#supported-data-providers). Works natively for properties of type `string`. For other types, MagiQuery will attempt to convert the property value to `string` during the filtering operation. |


### Supported Data Providers

Here is a table showing which properties, operators and features of MagiQuery are supported for each provider:

| Name                                        | Source           | Properties                                                                                                                     | Operators                                          | Nullables         | Supports String  Comparison Override? |
|---------------------------------------------|------------------|--------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------|-------------------|---------------------------------------|
| `Microsoft .EntityFrameworkCore .SqlServer` | `MSSQL`          | All                                                                                                                            | All, except `Regex`                                | Full              | No                                    |
| `Microsoft .EntityFrameworkCore .Sqlite`    | `SQLite`         | All, except `ulong`,  `DateTimeOffset`  and `TimeSpan`.  Type  `decimal`  is not  supported  for sorting                       | All                                                | Full              | No                                    |
| `Microsoft .EntityFrameworkCore .InMemory`  | `In-Memory DB`   | All                                                                                                                            | All                                                | Full              | No                                    |
| `Npgsql .EntityFrameworkCore .PostgreSQL`   | `PostgreSQL`     | All, except `DateOnly`  and `TimeOnly`                                                                                         | All                                                | Full              | No                                    |
| `Pomelo .EntityFrameworkCore .MySql`        | `MySQL, MariaDB` | All, except `DateOnly`  and `TimeOnly`                                                                                         | All                                                | Full              | No                                    |
| `MySql .EntityFrameworkCore`                | `MySQL`          | All, except `DateOnly`  and `TimeOnly`                                                                                         | All, except  `StarsWith`,  `EndsWith` and  `Regex` | Full              | No                                    |
| `MongoDB .EntityFrameworkCore`              | `MongoDB`        | All. Limited support for  string comparisons for  `TimeSpan` and `char`  (stored as numbers by  default) and  `DateTimeOffset` | All                                                | Base  Level  Only | No                                    |
| `Runtime`                                   | `IEnumerable`    | All                                                                                                                            | All                                                | Full              | Yes                                   |

### Nested and Nullable Properties

MagiQuery supports queries on nested properties. You can use the `.` operator to go down a level and access a property of a property. For example:

Let's say we are querying a collection with a base class named `Goblin`, which has a property named `Contract`. We can go deeper into the structure of `Contract`, and filter based on lower-level properties, like this:

`"property": "Contract.Details.SigningTime"`

This property name informs MagiQuery to filter the collection based on the value of the property `SigningTime`, which is a property of `Details`, which in turn is a property of `Contract`, which in turn is a property of the main `Goblin` class.

MagiQuery will automatically add all necessary null checks to the query as it traverses down the structure of the main class to reach the requested filter property.

Explicit null checks in MagiQuery can be performed by simply omitting the `"value"` property in your filter definition and using an `Equals` or `DoesNotEqual` operator:

```json5
    //Filters for entries where Contract is null
    {
      "property": "Contract",
      "operator": "Equals"
    }
```

```json5
    //Filters for entries where Contract is NOT null
    {
      "property": "Contract",
      "operator": "DoesNotEqual"
    }
```

## Custom Filter Expressions

The `QueryRequest` class allows you to define a custom logical expression that determines how the specified filters in the request are actually applied to the target collection.

Each filter defined in the `filters` collection in a `QueryRequest` is essentially a boolean condition.

Using logical operators, you can join together these boolean conditions to create a filtering logic that suits your needs.

There are three building blocks you can use to generate these expressions:

1. **Filter Index Numbers** - since `filters` is a collection, each individual filter has a unique 0-based indexing number. In your filter expression, you can use this indexing number to refer to a specific filter.


2. **Logical Operators** - MagiQuery supports three logical operators:

    - `&&` - this is the AND operator
    - `||` - this is the OR operator
    - `!` - this is the NOT operator


3. **Regular Brackets** - you can use regular brackets `()` to isolate specific evaluations to ensure they are executed in the intended order

Here is an example of how you can use a custom regular expression:

Let's say we want to filter for goblins that are:
- Born before January 1st, 2000

OR

- Have `MagicPower` of 4600000000 or less, and an `Id` that does not contain the digit `2`

We can achieve this with the following `QueryRequest`:

```json5
{
  "filters": [
    {
      "operator": "GreaterThan",
      "value": "4600000000",
      "property": "MagicPower"
    },
    {
      "operator": "DoesNotContain",
      "value": "2",
      "property": "Id"
    },
    {
      "operator": "LessThan",
      "value": "2000-01-01",
      "property": "DateOfBirth"
    }
  ],
  "filterExpression": "2 || (!0 && 1)"
}
```
Let's break down what is happening in the filter expression `2 || (!0 && 1)`:

- **2** - this is a reference to the third filter in the `filters` collection (the filter on `DateOfBirth`)
- **||** - this is an OR operator which ensures that either **2** or the result of the expression to the right of the `||` symbol needs to be `true` to evaluate the whole expression as `true`
- **(** - a bracket is opened, which will allow us to evaluate all expressions within the bracket as a whole
- **!0** - here we see a reference to the first filter in the `filters` collection (the filter on `MagicPower`). However, we see that the NOT (`!`) operator is applied to this filter, meaning we are reversing the result of the boolean expression (i.e. we are looking for `MagicPower` less than or equal to 4600000000, instead of greater than 4600000000)
- **&&** - this in an AND operator binding the result of the `!0` evaluation to the successful evaluation of the expression to the right of the `&&` symbol
- **1** - this is a reference to the second filter in the `filters` collection (the filter on `Id`). As a result of the preceding `&&` symbol, both this filter and the previous one need to evaluate as `true` to evaluate the whole expression in the brackets as `true`
- **)** - we are closing the brackets on the expression.

After applying this custom filter expression, the query result will only contain goblins that are either born before January 1st, 2000, OR have ids that do not contain the digit `2` and have 4600000000 magic power or less.

More examples can be seen in the [Examples](#examples) section.

#### Shorthand Expressions

MagiQuery offers two shorthand expressions for the most common query scenarios:

1. `"filterExpression": "&&"` - with this expression all `filters` will be joined together with an AND logical operator. Basically, this expression tells MagiQuery to perform a traditional filter operation. This is the default value of `filterExpression`.


2. `"filterExpression": "||"` - with this expression all `filters` will be joined together with an OR logical operator. This is the equivalent of a traditional search operation (provided each filter uses the `Contains` filter operator in its definition).

## Configuring Query Options

MagiQuery provides granular control over the query build process via the `QueryRequestOptions` class. You can pass an instance of `QueryRequestOptions` as an optional parameter to the `ApplyQuery` and `GetPagedResponse` extension methods.

### Excluding Properties

`ExcludedProperties` allows you to specify which properties of the `TItem` base class in `IQueryable<TItem>` you would like to explicitly exclude from the query.

For example, here is how you can exclude the properties `Name` and `DateOfBirth` from a query on class `Goblin`:

```csharp
QueryBuildOptions<Goblin> options = new()
{
    ExcludedProperties = [
        x=>x.Name,
        x=>x.DateOfBirth
    ]
};
```

If [IncludedProperties](#including-only-allowed-properties) is defined, it will override `ExcludedProperties`.

### Including ONLY Allowed Properties

`IncludedProperties` allows you to specify which properties of the `TItem` base class in `IQueryable<TItem>` are allowed to be queried.

If `IncludedProperties` is defined, MagiQuery will throw an error, if filtering or sorting is attempted on a property that has not been defined explicitly in `IncludedProperties`.

For example, here is how you can force MagiQuery to only allow filtering and sorting on the `Goblin` class' properties `Id`, `ExperiencePoints` and `Age`:

```csharp
QueryBuildOptions<Goblin> options = new()
{
    IncludedProperties = [
        x=>x.Id,
        x=>x.ExperiencePoints,
        x=>x.Age
    ]
};
```

When defined, `IncludedProperties` will override [ExcludedProperties](#excluding-properties).

### Property Name Mapping

Via `PropertyMapping`, you can specify an alias mapping for the properties of the class `TItem` in `IQueryable<TItem>`.

This is very useful, if you would like to hide the structure of the base class, or if you just want to add a developer-friendly nickname for a property, which might have a very long name (for example, a nested property which goes several levels below the base class).

Here is an example of a mapping that will add an alias for the nested property `Contract.Details.SigningTime`, as well as one for the properties `DateOfBirth` and `Id`:

```csharp
QueryBuildOptions<Goblin> options = new()
{
    PropertyMapping = new()
    {
        {"DOB", x=>x.DateOfBirth},
        {"SigningTime", x=>x.Contract.Details.SigningTime},
        {"GoblinId", x=>x.Id}
    }
};
```

Having provided this mapping, the FE can pass a payload like this:

```json5
{
  "filters": [
    {
        "operator": 4,
        "value": "2022-06-15T12:34:56Z",
        "property": "DOB"
    },
    {
        "operator": 11,
        "value": null,
        "property": "GoblinName"
    }
  ],
  "sorts": [
    {
        "descending": true,
        "property": "SigningTime"
    }
  ]
}
```

... and MagiQuery will correctly traverse the base class to find the correct properties to do filtering and sorting on.

### Hiding Mapped Properties

When [PropertyMapping](#property-name-mapping) has been defined, you can control whether properties with mapped names can be directly accessed by their actual name. This can be done with the boolean flag `HideMappedProperties`.

When `true`, a query can only access a property via its alias (if one has been defined). When `false`, a property can be accessed either by its alias or by its actual name.

`HideMappedProperties` is `true` by default.

### Override DateTimeKind

The `OverrideDateTimeKind` property allows you to override the `DateTimeKind` used for constructing filter constants of `DateTime` and `DateTimeOffset` during the query build.

This is especially useful when using PostgreSQL, as your queries may crash if you use a `DateTime` with `DateTimeKind.Unspecified` to filter on a column with type `timestamptz`.

### Specifying `BindingFlags` for Property Matching

`PropertyBindingFlags` allows you to specify the `BindingFlags` to be used during the property name parsing phase of the query build. [Here](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.bindingflags) you can read more about `BindingFlags`.

By default, `PropertyBindingFlags` has the value:

```csharp
 BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
```

Please be careful, if you decide to override the default `PropertyBindingFlags` value, as you may inadvertently expose properties that should not be accessible from outside your application.

### Configuring `StringComparison` per Query

🔴 **IMPORTANT**

*The `StringComparisonType` option is supported only for queries on collections that have already been fully loaded into memory, i.e. data provider is [Runtime](#supported-data-providers).*

*Unsupported if you are using a remote source like a [database](#supported-data-providers).*

The `StringComparisonType` property allows you to specify the `StringComparison`/`StringComparer` to be used during filtering and sorting of IQueryables coming from the `Runtime` data provider. Here are the supported values:

```csharp
- StringComparison.Ordinal (default)
- StringComparison.OrdinalIgnoreCase
- StringComparison.InvariantCulture
- StringComparison.InvariantCultureIgnoreCase
- StringComparison.CurrentCulture
- StringComparison.CurrentCultureIgnoreCase
```

## Override CultureInfo and Exact Parse Format

MagiQuery also offers the option to provide an override `CultureInfo` code and an exact parse format to ensure the string `value` you provide for a filter is parsed correctly by the .Net application.

Here is how you can do this at filter or sort level:

```json5
{
  "filters": [
    {
      "property": "DateOfBirth",
      "operator": "GreaterThan",
      "value": "1030 ЮНИ", // Raw Value
      "overrideCulture": "bg-BG", // Specifying culture to be used
      "exactParseFormat": "yyyy MMMM" // Specifying exact parse format for the value
    },
    {
      "property": "Agility",
      "operator": "LessThan",
      "value": "8,0",
      "overrideCulture": "bg-BG"
    }
  ]
}
```

If you want to override the `CultureInfo` on a `QueryRequest` level, you can rewrite the payload above like this:

```json5
{
  "filters": [
    {
      "property": "DateOfBirth",
      "operator": "GreaterThan",
      "value": "1030 ЮНИ",
      "exactParseFormat": "yyyy MMMM"
    },
    {
      "property": "Agility",
      "operator": "LessThan",
      "value": "8,0"
    }
  ],
  "overrideCulture": "bg-BG"
}
```

.. and the `CultureInfo` code will be used for all filter and sort definitions.

Even if a request-wide `CultureInfo` override is in effect, you can still override the `CultureInfo` code at the level of an individual filter or sort.

### Extended Capabilities for `Runtime` IQueryables

If the collection you are querying has already been fully loaded into memory (i.e. you are using the [Runtime](#supported-data-providers) data provider), the `overrideCulture` and `exactParseFormat` properties are also applied to the members of the collection.

This means that you can do the following:

```json5
{
  "filters": [
    {
      "property": "DateOfBirth",
      "operator": "Contains",
      "value": "er 1",
      "exactParseFormat": "MMMM yyyy"
    }
  ],
  "overrideCulture": "de-DE"
}
```

Which will return all entries where the month name ends in "er" and the first digit of the year is "1".

🔴 **IMPORTANT** You cannot do this directly with a remote [data provider](#supported-data-providers) like a database.

If you still want the same functionality with a database, you will first need to query the database to load all relevant entities into a collection in the memory, and then cast the collection as an IQueryable, before applying the `QueryRequest`. Like this:

```csharp
[Route("Goblins")]
public class QueryController(TestDbContext context) : ControllerBase
{
    [HttpPost("Query")]
    [ProducesResponseType<QueryResponsePaged<Goblin>>(StatusCodes.Status200OK)]
    public IActionResult QueryGoblins([FromBody] QueryRequestPaged request)
    {
        var goblins = context.Goblins.ToArray();
        return Ok(goblins.AsQueryable().GetPagedResponse(request));
    }
}
```

Note that this approach can be extremely resource-intensive and should be avoided whenever possible.

If you have no other option but to use it, at least try to narrow down the amount of entries you will need to load into memory to minimize the resource drain on each call.

## Examples

The following examples are available as query templates in the `WebApiExample` provided in this repository.

The `WebApiExample` comes with preloaded test data, stored in an in-memory SQLite database. It has a Swagger implementation with the payloads below listed as examples.

### Single Filter

Here is an example of a single `Equals` filter on a single property:

```json
{
  "filters": [
    {
      "property": "Name",
      "operator": "Equals",
      "value": "Sigurður"
    }
  ]
}
```

### Multiple Filters with AND operator

Here is an example with multiple filters on a single property with an AND operator. In this case, both conditions need to be satisfied for an entry to appear in the query result (`&&` operator).

```json
{
  "filters": [
    {
      "property": "Name",
      "operator": "StartsWith",
      "value": "B"
    },
    {
      "property": "Name",
      "operator": "Contains",
      "value": "r"
    }
  ]
}
```

### Multiple Filters with OR operator

Here is an example with multiple filters on a single property, joined with an OR operator. In this case only one of the conditions need to be satisfied for an entry to be included in the result (`||` operator).

```json
{
  "filters": [
    {
      "property": "Name",
      "operator": "StartsWith",
      "value": "B"
    },
    {
      "property": "Name",
      "operator": "Contains",
      "value": "r"
    }
  ],
  "filterExpression": "||"
}
```

### Multiple Filters with Complex Logic

Here is an advanced scenario in which we not only have multiple filters, but also a complex logic for including entries. Specifically, we would like to get goblins, whose name starts with "B", and are over 30-years-old, OR goblins, who were born before July 11th 1996, and do not have bitter taste.

```json
{
  "filters": [
    {
      "property": "Name",
      "operator": "StartsWith",
      "value": "B"
    },
    {
      "property": "Age",
      "operator": "GreaterThan",
      "value": "30"
    },
    {
      "property": "DateOfBirth",
      "operator": "LessThanOrEqual",
      "value": "1996-07-11 21:42:09.000"
    },
    {
      "property": "Taste",
      "operator": "DoesNotEqual",
      "value": "Bitter"
    }
  ],
  "filterExpression": "(0 && 1) || (2 && 3)"
}
```

### Multiple Filters with NOT operator

In addition to the AND (`&&`) and OR (`||`) operators, filter expressions also support the NOT (`!`) operator. The `!` operator allows you to reverse the condition of a filter without changing the filter operator. As you can see in this example, we filter for `IsActive=true`, but reverse it with `!` in the `filterExpression`.

```json
{
  "filters": [
    {
      "property": "isActive",
      "operator": "Equals",
      "value": "true"
    },
    {
      "property": "Strength",
      "operator": "GreaterThan",
      "value": "5.1"
    }
  ],
  "filterExpression": "!0 && 1"
}
```

### Filters on Nested and Nullable Properties

MagiQuery supports both nested and nullable properites.

Nested properties are separated from their parents with dots.

If you want to do a null evaluation on a property, you just need to specify the operator and omit the filter's `value` property in your `QueryRequest` payload.

```json
{
  "filters": [
    {
      "property": "Contract.Details.SigningTime",
      "operator": "GreaterThan",
      "value": "00:15:00"
    },
    {
      "property": "Contract",
      "operator": "DoesNotEqual"
    },
    {
      "property": "Contract.Details.DaysOfEffect",
      "operator": "Equals"
    }
  ]
}
```

### Search on Multiple Properties

Search is actually a variation of filtering. Just specify the properties you want to search on as filters with the `Contains` operator and use `||` in the `filterExpression`.

```json
{
  "filters": [
    {
      "property": "Name",
      "operator": "Contains",
      "value": "ee"
    },
    {
      "property": "FavouriteLetter",
      "operator": "Contains",
      "value": "ee"
    },
    {
      "property": "Taste",
      "operator": "Contains",
      "value": "ee"
    }
  ],
  "filterExpression": "||"
}
```

### Single Sort

Here is an example of how you can sort entries by a property. The default order is ascending, unless specified otherwise with `"descending": "true"`

```json
{
  "filters": [
    {
      "property": "Name",
      "operator": "Contains",
      "value": "e"
    }
  ],
  "sorts": [
    {
      "property": "FavouriteLetter"
    }
  ]
}
```

### Multiple Sorts

Here is an example of how you can sort entries by multiple properties. In this case, we are first sorting by date of birth in ascending order, and then by intelligence level in descending order.

```json
{
  "filters": [
    {
      "property": "Salary",
      "operator": "GreaterThanOrEqual",
      "value": "54001"
    }
  ],
  "sorts": [
    {
      "property": "DateOfBirth"
    },
    {
      "property": "IntelligenceLevel",
      "descending": true
    }
  ]
}
```

## License

MIT



