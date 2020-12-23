#拉取runtime父镜像
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
#设置容器工作目录
WORKDIR /publish
#把当前目录的所有文件copy到工作目录中
COPY . /publish
#暴露一个端口让外部可以访问
EXPOSE 5000
#容器入口命令
ENTRYPOINT ["dotnet", "Phenix.Services.Host.dll"]