{*******************************************************************************
 *
 * Unit Name : Log.sql
 * Purpose   : Log
 * Author    : ming
 * Maintainer:
 * History   :
 *
 * CreateDate: 2019-01-23
 *******************************************************************************}
 
CREATE TABLE PH7_EventLog_1/2/3/4 (       --事件日志(4个季度)
  EL_ID NUMERIC(15) NOT NULL,
  EL_Time DATE NOT NULL,                  --时间
  EL_ClassName VARCHAR(255) NULL,         --类名
  EL_MethodName VARCHAR(255) NULL,        --方法名
  EL_Message LONG NULL,                   --消息
  EL_ExceptionName VARCHAR(255) NULL,     --错误名/消息名
  EL_ExceptionMessage VARCHAR(4000) NULL, --错误消息/补充消息
  EL_User VARCHAR(500) NULL,              --用户(US_Name)
  EL_Address VARCHAR(39) NULL,            --IP地址
  EL_Trace_Key NUMERIC(15) NULL,          --调用链键值
  EL_Trace_Order NUMERIC(10) NULL,        --调用链顺序
  PRIMARY KEY(EL_ID)
)
/
