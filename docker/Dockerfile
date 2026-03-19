# ── Stage 1: build and publish the test project ──────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Copy solution structure
COPY Directory.Build.props ./
COPY src/ ./src/
COPY samples/ ./samples/

# Restore dependencies (cached layer unless .csproj files change)
RUN dotnet restore samples/Company.Automation.SampleTests/Company.Automation.SampleTests.csproj

# Build the test project and all its dependencies
RUN dotnet build samples/Company.Automation.SampleTests/Company.Automation.SampleTests.csproj \
    --no-restore \
    --configuration Release

# ── Stage 2: runtime image with Playwright dependencies ───────────────────────
# Use the official Playwright image for .NET — it includes all browser binaries
# and the OS-level dependencies (libnss, libx11, fonts, etc.) required in containers.
FROM mcr.microsoft.com/playwright/dotnet:v1.44.0-jammy AS runtime
WORKDIR /tests

# Copy the built test output from the build stage
COPY --from=build /repo/samples/Company.Automation.SampleTests/bin/Release/net8.0/ ./

# Create directories for Allure results and logs
RUN mkdir -p allure-results logs traces videos

# Environment defaults — override at runtime via docker run -e or docker-compose
ENV ASPNETCORE_ENVIRONMENT=QA
ENV AUTOMATION_Browser__Headless=true
ENV AUTOMATION_Report__AllureResultsDirectory=/tests/allure-results

# The entry point runs the tests with dotnet test (via the published DLL)
# Results land in /tests/allure-results — mount a volume to extract them:
#   docker run -v $(pwd)/allure-results:/tests/allure-results company-tests
ENTRYPOINT ["dotnet", "test", "Company.Automation.SampleTests.dll", \
            "--logger", "trx;LogFileName=results.trx", \
            "--results-directory", "/tests/allure-results"]

# ── Notes ─────────────────────────────────────────────────────────────────────
# To run specific tests: append --filter "TestCategory=Smoke" to docker run
# To change environment: docker run -e ASPNETCORE_ENVIRONMENT=QA company-tests
# To collect Allure results: mount /tests/allure-results to host path
#   then run: allure generate allure-results --clean -o allure-report
