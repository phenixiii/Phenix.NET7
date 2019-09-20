#拉取runtime父镜像，运行aspnet core应用必须要用runtime
FROM microsoft/dotnet:2.1-aspnetcore-runtime
#设置容器工作目录
WORKDIR /Phenix.Services.Host
#把当前目录的所有文件copy到工作目录中
COPY . /Phenix.Services.Host
#暴露一个端口让外部可以访问
EXPOSE 5000
#容器入口命令，即容器启动时执行dotnet Phenix.Services.Host.dll命令
ENTRYPOINT ["dotnet", "Phenix.Services.Host.dll"]