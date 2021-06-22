{*******************************************************************************
 *
 * Unit Name : App.sql
 * Purpose   : App
 * Author    : ming
 * Maintainer:
 * History   :
 *
 * CreateDate: 2020-08-30
 *******************************************************************************}

CREATE TABLE PH7_AppSettings (              --应用配置
  AS_Key VARCHAR(255) NOT NULL,             --键
  AS_Value VARCHAR(4000) NOT NULL,          --值      
  AS_ValueEncrypted NUMERIC(1) NOT NULL,    --是否加密
  PRIMARY KEY(AS_Key)
)
/
