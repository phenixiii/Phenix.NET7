{*******************************************************************************
 *
 * Unit Name : Message.sql
 * Purpose   : Message
 * Author    : ming
 * Maintainer:
 * History   :
 *
 * CreateDate: 2019-02-15
 *******************************************************************************}

CREATE TABLE PH7_UserMessage (       --用户消息
  UM_ID NUMERIC(15) NOT NULL,
  UM_Sender VARCHAR(500) NOT NULL,   --发送用户(US_Name)
  UM_Receiver VARCHAR(500) NOT NULL, --接收用户(US_Name)
  UM_CreateTime DATE NOT NULL,       --创建时间
  UM_SendTime DATE NULL,             --发送时间(同ID最后一次刷新时间)
  UM_ReceivedTime DATE NULL,         --收到时间
  UM_Content LONG NULL,              --消息内容(未加校验码和头尾标志)
  PRIMARY KEY(UM_ID)
)
/

