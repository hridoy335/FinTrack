| Branch | Status                                                                                                            |
|--------|-------------------------------------------------------------------------------------------------------------------|
| main   | ![Dotnet 8](https://github.com/md-redwan-hossain/SharpOutcome/actions/workflows/dotnet.yml/badge.svg?branch=main) |

- [Installation](#installation)
- [Introduction](#introduction)
- [Potential Corner Cases](#potential-corner-cases)
- [Use Cases](#use-cases)
- [Available API](#available-api)
    - [`Match` and `MatchAsync`](#match-and-matchasync)
    - [`Switch` and `SwitchAsync`](#switch-and-switchasync)
    - [`IsGoodOutcome` and `IsBadOutcome`](#isgoodoutcome-and-isbadoutcome)
    - [`TryPickGoodOutcome(out TGoodOutcome? goodOutcome)`](#trypickgoodoutcomeout-tgoodoutcome-goodoutcome)
    - [
      `TryPickGoodOutcome(out TGoodOutcome? goodOutcome, out TBadOutcome? badOutcome)`](#trypickgoodoutcomeout-tgoodoutcome-goodoutcome-out-tbadoutcome-badoutcome)
    - [`TryPickBadOutcome(out TBadOutcome? badOutcome)`](#trypickbadoutcomeout-tbadoutcome-badoutcome)
    - [
      `TryPickBadOutcome(out TGoodOutcome? goodOutcome, out TBadOutcome? badOutcome)`](#trypickbadoutcomeout-tgoodoutcome-goodoutcome-out-tbadoutcome-badoutcome)
- [Helpers](#helpers)
- [Example Code Snippets](#example-code-snippets)

---

### Installation

To install, run `dotnet add package SharpOutcome` or from [Nuget](https://www.nuget.org/packages/SharpOutcome/)

---

### Introduction

`SharpOutcome` offers an implementation of the Result pattern featuring a straightforward API, enabling seamless code
flow management without the need for exceptions.

There are two main types:

- `Outcome<TGoodOutcome, TBadOutcome>` which is a `class`.
- `ValueOutcome<TGoodOutcome, TBadOutcome>` which is a `readonly record struct`.

Both of them represent an outcome that can either be a good outcome of type `TGoodOutcome` or a bad outcome of
type `TBadOutcome`. For `TGoodOutcome` and `TBadOutcome`, you can use any **non-nullable type**. **Non-nullable** means
you can use `FooBar`but not `FooBar?` or `Nullable<FooBar>`. This is intentional because nullability can be treated as a
bad outcome. For convenience, [helper interfaces along with their concrete implementations](#helpers) are provided.

Both types allow constructor creation or implicit conversion. For example, if a method has return
type `Task<ValueOutcome<IGoodOutcome, IBadOutcome>>`, you can use the following patterns. Implicit return is preferred
for cleaner code.

```cs
// use of constructor
return new ValueOutcome<IGoodOutcome, IBadOutcome>(new GoodOutcome(GoodOutcomeTag.Deleted));
// return based on implicit conversion
return new GoodOutcome(GoodOutcomeTag.Deleted);
```

---

### Potential Corner Cases

- Since `TGoodOutcome` and `TBadOutcome` can take any **non-nullable type**, it is your responsibility to use them
  properly. No one will stop you from flipping the semantics. For example, you can use any **non-nullable type**
  for `TBadOutcome` that is meant for something good or success, but you shouldn't.

- If you need to use nullable type, wrap it with `NullablePrimitive` or `NullableNonPrimitive` as per your need.

- `TGoodOutcome` and `TBadOutcome` must be of different type. For example, if a method has `Outcome<string, string>`
  as the return type, what's the benefit? In this case, you can just simply use `string` as return type.

- `ValueOutcome` is added for lowering the pressure on the garbage collector. Since C# enforces a parameterless
  constructor for `struct` type, no one will stop you doing the following:

```csharp
ValueOutcome<string, int> MisuseOfValueOutcome()
{
    return new ValueOutcome<string, int>();
}
```

But you will get `InvalidOperationException` at runtime. If you want to enforce it in compile time, simply
use `Outcome` type which has no parameterless public constructor.

The proper use of `ValueOutcome` should be the following:

```csharp
ValueOutcome<string, int> ProperUseOfValueOutcome()
{
    if (RandomNumberGenerator.GetInt32(1, 10) == 5)
    {
        return "Ok";
    }

    return -1;
}
```

- Be careful when you use implicit conversion return feature from method. In the following code, you are returning `int`
  twice. If you are expecting BadOutcome as `string`, this will never happen because nothing is returned which
  has `string` as data type.

```csharp

ValueOutcome<int, string> Gotcha()
{
    int number = RandomNumberGenerator.GetInt32(1, 10);

    if (number % 2 == 0)
    {
        return number;
    }

    return number;
}
```

---

### Use Cases

- As method parameter value.
- Method return value.
- A complete REST API with CRUD functionality example is also given to showcase the usefulness of SharpOutcome. Source
  code is available [here.](https://github.com/md-redwan-hossain/SharpOutcome/tree/main/src/SharpOutcome.HttpApiExample)
- [Example code snippets](#example-code-snippets).

---

### Available API

#### `Match` and `MatchAsync`

The `Match` and `MatchAsync` methods execute a function on the good or bad outcome and return the result. If the outcome
is bad, the function for the bad outcome is executed; otherwise, the function for the good outcome is executed.

```csharp
return await result.MatchAsync<IActionResult>(
    entity => ResponseMakerAsync<Book, BookResponse>(HttpStatusCode.OK, entity),
    err => ResponseMaker(err)
);
```

---

#### `Switch` and `SwitchAsync`

The `Switch` and `SwitchAsync` methods execute an action on the good or bad outcome. If the outcome is bad, the action
for the bad outcome is executed; otherwise, the action for the good outcome is executed. These methods do not return
anything.

```csharp
await result.SwitchAsync(
    entity => SendOkAsync(HttpStatusCode.OK, entity),
    err => SendBadRequestAsync(err)
);
```

---

#### `IsGoodOutcome` and `IsBadOutcome`

The `IsGoodOutcome` and `IsBadOutcome` get-only properties denote the status of the resolved outcome.

#### `TryPickGoodOutcome(out TGoodOutcome? goodOutcome)`

This method tries to extract the good outcome from the `Outcome` instance. If the instance represents a good outcome, it
assigns the good outcome to the `goodOutcome` out parameter and returns true. If the instance represents a bad outcome,
it assigns the default value to the `goodOutcome` out parameter and returns false.

```csharp
var checkConfirmation = await CheckConfirmation(dto.Id);

if (checkConfirmation.TryPickGoodOutcome(out var goodOutcome))
{
    return new GoodOutcome(GoodOutcomeTag.Valid, goodOutcome.ToString());
}
```

#### `TryPickGoodOutcome(out TGoodOutcome? goodOutcome, out TBadOutcome? badOutcome)`

This overload of `TryPickGoodOutcome` tries to extract both the good and bad outcomes from the `Outcome` instance. If
the instance represents a good outcome, it assigns the good outcome to the `goodOutcome` out parameter, the default
value to the `badOutcome` out parameter, and returns true. If the instance represents a bad outcome, it assigns the
default value to the `goodOutcome` out parameter, the bad outcome to the `badOutcome` out parameter, and returns false.

```csharp
var result =  Demo.IdSender();

return result.TryPickGoodOutcome(out var good, out var bad)
    ? Results.Ok(good)
    : Results.BadRequest(bad);
```

#### `TryPickBadOutcome(out TBadOutcome? badOutcome)`

Functionality is same as [`TryPickGoodOutcome`](#trypickgoodoutcomeout-tgoodoutcome-goodoutcome)but it tries to extract
the bad outcome instead of the good outcome.

#### `TryPickBadOutcome(out TGoodOutcome? goodOutcome, out TBadOutcome? badOutcome)`

Functionality is same
as [`TryPickGoodOutcome`](#trypickgoodoutcomeout-tgoodoutcome-goodoutcome-out-tbadoutcome-badoutcome)but it tries to
extract the bad outcome instead of the good outcome.

---

### Helpers

For convenience, `IGoodOutcome`, `IBadOutcome`, `IGoodOutcomeWithPayload`, `IBadOutcomeWithPayload` interfaces are
provided along with their concrete
implementations `GoodOutcome`,`BadOutcome`, `GoodOutcomeWithPayload`, `BadOutcomeWithPayload`.

```csharp
public interface IGoodOutcome
{
    GoodOutcomeTag Tag { get; }
    string? Reason { get; }
}


    public interface IGoodOutcome<out TOutcomeTag>
{
    TOutcomeTag Tag { get; }
    string? Reason { get; }
}
```

```csharp
public interface IGoodOutcomeWithPayload<out TPayload>
{

    GoodOutcomeTag Tag { get; }
    TPayload Payload { get; }
    string? Reason { get; }
}


public interface IGoodOutcomeWithPayload<out TPayload, out TOutcomeTag>
{
    TOutcomeTag Tag { get; }
    TPayload Payload { get; }
    string? Reason { get; }
}
```

```csharp
public interface IBadOutcome
{
    BadOutcomeTag Tag { get; }
    string? Reason { get; }
}


public interface IBadOutcome<out TOutcomeTag>
{
    TOutcomeTag Tag { get; }
    string? Reason { get; }
}
```

```csharp
public interface IBadOutcomeWithPayload<out TPayload>
{
    BadOutcomeTag Tag { get; }
    TPayload Payload { get; }
    string? Reason { get; }
}

public interface IBadOutcomeWithPayload<out TPayload, out TOutcomeTag>
{
    TOutcomeTag Tag { get; }
    TPayload Payload { get; }
    string? Reason { get; }
}
```

---

### Example Code Snippets

Here is an example with a service class method:

```csharp
public async Task<Outcome<Book, IBadOutcome>> UpdateAsync(int id, BookRequest dto)
{
    try
    {
        Book? entityToUpdate = await _bookDbContext.Books.FindAsync(id);
        if (entityToUpdate is null) return new BadOutcome(BadOutcomeTag.NotFound);

        await dto.BuildAdapter().AdaptToAsync(entityToUpdate);
        _bookDbContext.Books.Attach(entityToUpdate);
        _bookDbContext.Entry(entityToUpdate).State = EntityState.Modified;
        await _bookDbContext.SaveChangesAsync();
        return entityToUpdate;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return new BadOutcome(BadOutcomeTag.Unexpected);
    }
}
```

The service class can be consumed in a controller class like the following way using the `MatchAsync` method:

```csharp
[HttpPut("{id:int}")]
public async Task<IActionResult> PutBook(int id, BookRequest dto)
{
    if (!ModelState.IsValid) return ResponseMaker(HttpStatusCode.BadRequest);

    Outcome<Book, IBadOutcome> result = await _bookService.UpdateAsync(id, dto);

    return result.Match<IActionResult>(
        entity => ResponseMaker(HttpStatusCode.OK, entity),
        err => ResponseMaker(err)
    );
}

private IActionResult ResponseMaker(IBadOutcome error)
{
    var code = error.Tag switch
    {
        BadOutcomeTag.Failure => HttpStatusCode.InternalServerError,
        BadOutcomeTag.Unexpected => HttpStatusCode.InternalServerError,
        BadOutcomeTag.Validation => HttpStatusCode.BadRequest,
        BadOutcomeTag.Conflict => HttpStatusCode.Conflict,
        BadOutcomeTag.NotFound => HttpStatusCode.NotFound,
        BadOutcomeTag.Unauthorized => HttpStatusCode.Unauthorized,
        BadOutcomeTag.Forbidden => HttpStatusCode.Forbidden,
        _ => HttpStatusCode.InternalServerError,
    };

    return ResponseMaker(code, null, error.Reason);
}

private IActionResult ResponseMaker(HttpStatusCode code, object? data = null, string? message = null)
{
    if (code == HttpStatusCode.NoContent) return NoContent();

    var castedCode = (int)code;
    var isSuccess = castedCode is >= 200 and < 300;
    var res = new
    {
        Success = isSuccess,
        Message = message ?? ReasonPhrases.GetReasonPhrase(castedCode),
        Code = castedCode,
        Data = data
    };

    return StatusCode(castedCode, res);
}
```
