# Simulation .NET Backend Diploma - Unit Testing Assignment

A comprehensive unit testing, architectural guardrails, and code coverage assignment for a .NET Clean Architecture checkout service.

---

## 🛠️ Tech Stack & Libraries
* **Framework:** .NET 10.0 (`net10.0`)
* **Testing Framework:** xUnit
* **Mocking:** Moq
* **Assertions:** FluentAssertions
* **Test Data Generation:** AutoFixture & Bogus
* **Snapshot Testing:** Verify.Xunit
* **Architecture Rules:** NetArchTest.Rules
* **Code Coverage:** Coverlet & ReportGenerator

---

## 📋 Assignment Tasks Implemented

1. **Task 1 & 2: Unit Testing & Business Rules (Fail Fast)**
   - Tested checkout success paths and validation rules (e.g., handling insufficient inventory and invalid quantities).
   - Verified that dependencies (`IOrderRepository`, `INotificationService`, `IInventoryService`) are called or ignored appropriately (`Times.Once`, `Times.Never`).

2. **Task 3: Test Data Builders**
   - Utilized **AutoFixture** to automatically generate valid `Order` and `OrderItem` objects with customized positive constraints.
   - Utilized **Bogus** with a fixed seed (`UseSeed`) to generate realistic customer names and emails for deterministic test runs.

3. **Task 4: Snapshot Testing**
   - Implemented `Verify.Xunit` to snapshot checkout receipts using fixed data (Example C).
   - Committed the `.verified.txt` baseline to track structural receipt changes over time.

4. **Task 5: Architectural Guardrails**
   - Enforced Clean Architecture boundaries using `NetArchTest`:
     - Domain does not depend on Application or Infrastructure.
     - Application does not depend on Infrastructure.
     - All interfaces start with `I`.
     - Classes implementing `IOrderRepository` end with `Repository`.

5. **Task 6: Code Coverage Analysis**
   - Generated coverage reports using `Coverlet` and converted them to readable HTML/Text summaries via `ReportGenerator`.

---

## 🚀 How to Run Tests & Coverage

Run all unit tests:
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html;TextSummary"
```
