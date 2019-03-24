FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY XUnitTestProject1/XUnitTestProject1.csproj XUnitTestProject1/
RUN dotnet restore XUnitTestProject1/XUnitTestProject1.csproj
COPY . .
WORKDIR /src/XUnitTestProject1
RUN dotnet build XUnitTestProject1.csproj -c Release -o /app


# Mount a volume TestResults, so results can be read on completion
ENTRYPOINT ["dotnet", "test", "--logger", "trx;LogFileName=/TestResults/test-results.trx", "XUnitTestProject1.csproj"]
