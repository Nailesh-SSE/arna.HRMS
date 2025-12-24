# How to Test the EmployeeService

## What Are We Testing?

We are testing the EmployeeService class.

This class performs three main functions:

1. Interacts with the repository to access the database.
2. Uses AutoMapper for object mapping.
3. Returns EmployeeDto objects.

In unit testing, we focus on:

- Avoiding real database interactions.
- Testing only the service logic.

To achieve this, we use Moq to mock the database.

## Important Concept: Mocking the Database

- ❌ Real database: Not used.
- ✅ Fake database: Preferred.

Reasons:
- No need for SQL.
- No database tables required.
- Tests run faster.
- No connection strings needed.

We use: `Mock<IBaseRepository<Employee>>`

This tells C# to pretend we have a database and return values as specified.

## The 3 Steps to Write Any Test

Remember these three words:

1. Arrange
2. Act
3. Assert

### 1. Arrange (Setup / Prepare)

In this step:
- Create fake employees.
- Configure the mock repository to return specific values.

Example:

```csharp
var employee = new Employee { Id = 1, FirstName = "John" };
_baseRepositoryMock.Setup(r => r.AddAsync(employee))
                   .ReturnsAsync(employee);
```

This means: If someone tries to add this employee, pretend it succeeds and return it.

### 2. Act (Call the Method)

Call the service method you want to test:

```csharp
var result = await _employeeService.CreateEmployeeAsync(employee);
```

This is the "action" step.

### 3. Assert (Check the Result)

Verify if the result is correct.

Example:

```csharp
Assert.That(result.FullName, Is.EqualTo("John Doe"));
```

This checks: Did the service produce the expected output?

## In Simple English

Testing is like a science experiment:

- Arrange: 🥼 Set up your tools and materials.
- Act: ⚗️ Perform the experiment.
- Assert: 📏 Check the results.

## Why Mocking Is Needed?

When the service tries to save data:

```csharp
repository.AddAsync(employee);
```

We don't want:
- ❌ Real database.
- ❌ Real SQL.

So we mock it:

```csharp
_baseRepositoryMock.Setup(r => r.AddAsync(employee));
```

Moq pretends to be a database.

## How to Create Your Own Test

Think step by step.

Example: Test that employee salary should be updated.

1. Arrange:
   - Create fake employee: `var employee = new Employee { Salary = 1000 };`
   - Update salary: `employee.Salary = 5000;`
   - Mock update: `_baseRepositoryMock.Setup(r => r.UpdateAsync(employee)).ReturnsAsync(employee);`

2. Act:
   - `var result = await _employeeService.UpdateEmployeeAsync(employee);`

3. Assert:
   - `Assert.That(result.Salary, Is.EqualTo(5000));`

And you've created a test case!

## Thinking Pattern

Whenever you want to test:

"If I give THIS input… service should return THAT output."

Examples:
- If employee id = 1 → return John.
- If employee id not found → return null.
- If deleted id = 1 → return true.
- If deleted id = 99 → return false.
- If full name → "First Last".

Translate to:
- Arrange: Create that scenario.
- Act: Call the method.
- Assert: Verify expected outcome.

## Another Example: Delete Employee Fails

- Arrange: `_baseRepositoryMock.Setup(r => r.DeleteAsync(5)).ReturnsAsync(false);`
- Act: `var result = await _employeeService.DeleteEmployeeAsync(5);`
- Assert: `Assert.False(result);`

You've written another test case!

## Summary

- Fake the database using Moq.
- Test only the service logic.
- Always follow 3 steps: Arrange, Act, Assert.

## Practice by Writing Tests For

- Salary update
- Position update
- Invalid email
- Employees count
- Name mapping
- Delete fails
- Manager id is null

## Final Tip

When you think: "What should happen if…"

Turn that into:
- Arrange → Create that scenario
- Act → Call method
- Assert → Verify expected outcome
