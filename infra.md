Act as a senior .NET 8 automation architect and a hands-on test automation framework engineer.
Your task is to design and generate a production-grade, enterprise-ready C#/.NET 8 automation infrastructure for UI and API testing, intended to be consumed by manual testers with minimal coding effort.
The framework must be built only with proven, stable, free, and actively maintained technologies. Do not suggest paid tools. Use modern best practices, clean architecture, modular design, SOLID principles, strong separation of concerns, and maintainability as top priorities.
==================================================
1. PRIMARY GOAL
==================================================
Create a reusable automation framework in C# on .NET 8 that serves as a full infrastructure layer for:
- UI automation
- API automation
- Reporting
- Configuration
- Execution in local and containerized environments
- Packaging and publishing as an internal NuGet package
The framework is intended to provide a complete abstraction layer so that manual testers can write tests by supplying only:
- UI locators and test logic
or
- API endpoint details / payloads / assertions
The framework itself must encapsulate all technical complexity behind clean, easy-to-use functions.
==================================================
2. HIGH-LEVEL REQUIREMENTS
==================================================
The framework must include:
1. Full Playwright-based UI automation wrapper layer
2. API testing layer
3. Smart waits and resiliency mechanisms
4. Rich Allure reporting integration
5. Configurable driver/browser management
6. Headless and headed execution modes
7. Modular project structure based on best practices
8. Clean extension points for future growth
9. Internal NuGet packaging support
10. Container-ready execution
11. Environment-based configuration for:
  - Base UI URL
  - API base URLs
  - Browser settings
  - Timeouts
  - Retries
  - Environment names
12. A simple consumer experience for testers
==================================================
3. MANDATORY TECHNOLOGY STACK
==================================================
Use only free, verified, production-proven tools that work well with .NET 8.
Required stack:
- .NET 8
- C#
- Microsoft.Playwright
- xUnit or NUnit (choose the better option for enterprise maintainability and explain why)
- Allure reporting for .NET
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- System.Text.Json or Newtonsoft.Json (choose one and justify)
- RestSharp or HttpClient-based API layer (prefer the more robust long-term approach and explain why)
- FluentAssertions
- Docker
Optional but recommended if justified:
- Polly for retry/resilience
- Serilog for internal framework logging
- SourceLink
- Directory.Build.props
- GitHub Actions or Azure DevOps sample pipeline
Do not use Selenium.
Prefer Playwright for all UI automation.
==================================================
4. ARCHITECTURE REQUIREMENTS
==================================================
Design the solution as a modular multi-project solution with strong separation of responsibilities.
Suggested structure (you may improve it if justified):
- Company.Automation.Core
- Company.Automation.UI
- Company.Automation.API
- Company.Automation.Reporting
- Company.Automation.Configuration
- Company.Automation.Contracts
- Company.Automation.Testing or Company.Automation.TestHost
- Company.Automation.SampleTests
- Company.Automation.NuGetPackaging
If a better naming convention or project split exists, use it and explain the reasoning.
Each project must have a clear purpose.
==================================================
5. UI AUTOMATION REQUIREMENTS
==================================================
Build a full wrapper around Playwright capabilities so that the infrastructure hides implementation complexity.
The framework must provide wrapped functions for common and advanced Playwright actions, including but not limited to:
Core actions:
- Navigate to page
- Click
- Double click
- Right click
- Hover
- Fill text
- Clear text
- Press keys
- Select dropdown option(s)
- Check/uncheck checkbox
- Upload file
- Drag and drop
- Take screenshot
- Read text
- Read attribute
- Is visible / hidden / enabled / disabled / checked
- Wait for element states
- Wait for page load states
- Switch tabs/pages/windows
- Handle dialogs / alerts / confirmations
- Handle frames / iframes
- Keyboard actions
- Mouse actions
- Scroll actions
Advanced support:
- Smart wait before and after actions
- Retry/iteration logic for flaky UI timing issues
- Element stability checks
- Safe click with built-in verification
- Automatic detection of transient failures
- Friendly error messages
- Logging for each action
- Automatic attachment to Allure report
- Screenshot on failure
- Optional screenshot on success
- Trace or Playwright artifacts if useful
- Network idle / DOM ready / visible / attached / stable handling where relevant
For every wrapped UI action:
- The public API should be simple for the tester
- The infrastructure should handle retries, waits, validation, and reporting behind the scenes
Example expectation:
If the tester calls something like:
Click(locator)
The framework implementation should internally:
- Resolve locator safely
- Wait intelligently for visibility and interactability
- Retry with controlled policy if needed
- Perform the click
- Validate that the action was actually performed when possible
- Log technical details
- Report the step to Allure
- Capture screenshot or artifact on failure
- Throw a clear domain-specific exception if unsuccessful
Create this pattern consistently for all important UI capabilities.
==================================================
6. LOCATOR AND PAGE MODEL STRATEGY
==================================================
Use a maintainable and modular UI design pattern.
Choose and implement the most suitable approach among:
- Page Object Model
- Page Component Model
- Screenplay-inspired abstraction
- A hybrid best-practice model
The answer must include:
- Recommended structure
- Rationale
- Sample implementation
The framework should allow testers to work easily with locators while still encouraging maintainable page abstractions.
The solution must support:
- Centralized locator definitions
- Reusable components
- Common page methods
- Base page abstractions
- Cross-page shared behaviors
- Avoiding duplicate code
==================================================
7. API AUTOMATION REQUIREMENTS
==================================================
Provide a strong API testing layer for REST APIs.
Capabilities required:
- GET / POST / PUT / PATCH / DELETE
- Headers management
- Query parameters
- Path parameters
- Authentication support
- JSON serialization/deserialization
- Request/response logging
- Retry policy if relevant
- Timeout handling
- Response validation helpers
- Status code validation
- Body assertion helpers
- Schema validation if a free and reliable .NET option is appropriate
- Easy attachment of request and response details to Allure report
The consumer API should be simple for testers:
- Send request
- Validate response
- Attach evidence automatically
==================================================
8. REPORTING REQUIREMENTS
==================================================
Create a dedicated reporting layer for Allure.
This must include:
- Step-level reporting
- Automatic logging of action details
- Screenshot attachments
- API request/response attachments
- Failure attachments
- Optional environment details
- Categories / labels / metadata if applicable
- Helper classes for reusable reporting patterns
The reporting package/classes must be cleanly separated and reusable.
==================================================
9. CONFIGURATION REQUIREMENTS
==================================================
Create a robust configuration system using appsettings and environment-specific overrides.
Must support:
- appsettings.json
- appsettings.Development.json
- appsettings.QA.json
- appsettings.Production.json
- Environment variables override
- Command line override if useful
Configuration should include:
- UI base URL
- API base URL(s)
- Browser type
- Headless mode
- Default timeout
- Retry count
- Screenshot behavior
- Report settings
- Environment name
- Execution flags
Create strongly typed configuration classes and register them with dependency injection.
==================================================
10. BROWSER / DRIVER MANAGEMENT
==================================================
Implement browser lifecycle management properly.
Requirements:
- Support Chromium at minimum
- Optional support for Firefox/WebKit if practical
- Headless and headed mode
- Browser context configuration
- Per-test or shared context strategy (choose best practice and justify)
- Download handling
- Video / trace support if useful
- Clean disposal and teardown
- Parallel execution considerations
Design for reliable test execution and minimal flakiness.
==================================================
11. EXCEPTIONS, LOGGING, AND RESILIENCY
==================================================
The framework must include:
- Custom exception hierarchy
- Friendly errors for testers
- Technical logs for debugging
- Smart retry logic only where safe
- Timeout strategy
- Defensive coding against stale/transient state problems
- Clear separation between test failures and infrastructure failures
If using Polly, apply it carefully and only where appropriate.
==================================================
12. TESTER EXPERIENCE / CONSUMER EXPERIENCE
==================================================
This is critical.
The framework must be designed so that consumer test projects are very simple.
A tester using the internal NuGet package should only need to:
- Install the package(s)
- Add configuration file
- Write test logic
- Provide locators / endpoints / assertions
The tester should not need to understand Playwright internals.
Create a sample consumer test project demonstrating:
- One UI login test
- One UI form interaction test
- One API test
- One example mixing UI and API if relevant
Also provide examples of the intended easy-to-use public API.
==================================================
13. NUGET PACKAGING REQUIREMENTS
==================================================
The framework must be designed to be published as an internal organizational NuGet package.
Include:
- Proper csproj metadata
- Versioning approach
- Dependency management
- Symbols/source package if useful
- Clear packaging strategy
- Which projects become packages and which remain internal
- Example command/scripts to pack and publish to internal NuGet feed
The output must be suitable for enterprise reuse.
==================================================
14. CONTAINERIZATION REQUIREMENTS
==================================================
The framework must be runnable inside a container after the testers implement their tests.
Provide:
- Dockerfile
- Any Playwright dependencies required for Linux container execution
- Best-practice base image recommendation
- Instructions for running tests in container
- Notes about Allure result output path
- Considerations for CI/CD execution
Container execution must support headless browser mode.
==================================================
15. CI/CD READINESS
==================================================
Design the solution so it can be used in CI pipelines.
Include:
- Sample pipeline strategy
- Restore/build/test/report flow
- Artifact publishing guidance
- Allure results handling
- Container execution option
- NuGet packing/publishing stage
Use only realistic and maintainable recommendations.
==================================================
16. OUTPUT FORMAT REQUIRED FROM YOU
==================================================
Your response must be highly structured and include ALL of the following:
1. Executive architecture decision summary
2. Recommended solution structure
3. Detailed explanation of each project/module
4. Technology choices with justification
5. Folder structure
6. Code for key infrastructure pieces
7. Base abstractions and interfaces
8. Playwright wrapper implementation examples
9. API client implementation examples
10. Allure reporting helper implementation examples
11. Configuration model examples
12. Dependency injection setup
13. Sample test project
14. Dockerfile
15. NuGet packaging guidance
16. CI/CD sample
17. Best practices and trade-offs
18. Risks / limitations / future extension recommendations
==================================================
17. CODE QUALITY REQUIREMENTS
==================================================
All generated code must be:
- Realistic
- Buildable
- Consistent
- Production-oriented
- Well named
- Well organized
- Not pseudo-code unless explicitly marked as example
- Compatible with .NET 8
- Based on existing real libraries and valid APIs
Avoid vague descriptions.
Provide concrete code.
Do not invent fake libraries or fake APIs.
==================================================
18. DESIGN PREFERENCES
==================================================
Apply these principles:
- Clean architecture where practical
- SOLID
- DRY
- KISS
- Composition over inheritance where appropriate
- Minimal but powerful public API
- Maximum maintainability
- Enterprise-grade readability
- Extensibility without overengineering
==================================================
19. IMPORTANT FUNCTIONAL EXPECTATION
==================================================
The framework must systematically wrap Playwright capabilities with infrastructure behavior.
For example, every important UI action should include:
- Smart wait
- Retry if appropriate
- Validation
- Logging
- Allure reporting
- Failure evidence
- Clear exception strategy
This wrapping concept is the heart of the framework and must be implemented consistently, not just described conceptually.
==================================================
20. FINAL DELIVERABLE EXPECTATION
==================================================
Produce a complete, professional blueprint plus implementation skeleton that a development team can use immediately to build the internal automation framework and publish it as an organizational NuGet package.
Where there are multiple valid architecture choices, choose the best one for long-term enterprise maintainability and explicitly explain why.
Also include a final section titled:
"Open Questions / Missing Decisions"
and list any details that should be clarified before implementation begins.