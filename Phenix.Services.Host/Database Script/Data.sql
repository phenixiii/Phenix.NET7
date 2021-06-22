{*******************************************************************************
 *
 * Unit Name : Data.sql
 * Purpose   : Data
 * Author    : ming
 * Maintainer:
 * History   :
 *
 * CreateDate: 2019-03-13
 *******************************************************************************}

CREATE TABLE PH7_SequenceMarker (     --序号标识
  SM_ID NUMERIC(3) default 0 NOT NULL, --标识ID
  SM_Address VARCHAR(39) NOT NULL,     --IP地址
  SM_ActiveTime DATE NOT NULL,         --活动时间
  PRIMARY KEY(SM_ID),
  UNIQUE(SM_Address)
)
/
CREATE TABLE PH7_Increment (     --增量
  IC_Key VARCHAR(255) NOT NULL,  --键
  IC_Value NUMERIC(15) NOT NULL, --值
  IC_Time DATE NOT NULL,         --时间
  PRIMARY KEY(IC_Key)
)
/
