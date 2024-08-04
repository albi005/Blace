FROM mcr.microsoft.com/dotnet/sdk:latest

RUN dotnet workload install wasm-tools

RUN apt-get update
RUN apt-get install -y python3 python3-pip
RUN apt-get install -y procps

WORKDIR /src

CMD ["bash"]
