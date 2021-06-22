{*******************************************************************************
 *
 * Unit Name : Schema.sql
 * Purpose   : Schema
 * Author    : ming
 * Maintainer:
 * History   :
 *
 * CreateDate: 2020-09-17
 *******************************************************************************}

CREATE TABLE PH7_UndoLog_1/2/3/4 (         --回滚日志(4个季度)
  UL_ID NUMERIC(15) NOT NULL,
  UL_Time DATE NOT NULL,                   --时间
  UL_Transaction_Key NUMERIC(15) NOT NULL, --事务键值
  UL_TargetTable VARCHAR(30) NOT NULL,     --表名
  UL_TargetID NUMERIC(15) NOT NULL,        --主键
  UL_Timestamp NUMERIC(15) NOT NULL,       --时间戳
  UL_Redo_Content VARCHAR(4000) NOT NULL,  --提交语句
  UL_Undo_Content VARCHAR(4000) NOT NULL,  --回滚语句
  PRIMARY KEY(UL_ID)
)
/
