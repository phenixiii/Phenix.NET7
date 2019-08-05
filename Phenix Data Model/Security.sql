{*******************************************************************************
 *
 * Unit Name : Security.sql
 * Purpose   : Security
 * Author    : ming
 * Maintainer:
 * History   :
 *
 * CreateDate: 2019-02-28
 *******************************************************************************}

CREATE TABLE PH7_Teams (             --团体(树, 公司/部门/小组)
  TM_ID NUMERIC(15) NOT NULL,
  TM_Name VARCHAR(100) NOT NULL,     --名称
  TM_Root_ID NUMERIC(15) NOT NULL,   --顶层团体(=TM_ID:顶层)
  TM_Parent_ID NUMERIC(15) NOT NULL, --父层团体(=0:顶层)
  PRIMARY KEY(TM_ID),
  UNIQUE(TM_Root_ID, TM_Name),
  UNIQUE(TM_Parent_ID, TM_Name)
)
/
CREATE TABLE PH7_Position (        --岗位
  PT_ID NUMERIC(15) NOT NULL,
  PT_Name VARCHAR(100) NOT NULL,   --名称
  PT_Roles VARCHAR(4000) NOT NULL, --角色数组(逗号分隔)
  PRIMARY KEY(PT_ID),
  UNIQUE(PT_Name)
)
/
CREATE TABLE PH7_User (                                 --用户
  US_ID NUMERIC(15) NOT NULL,
  US_Name VARCHAR(100) NOT NULL,                        --登录名(/手机/邮箱)
  US_Phone VARCHAR(13) NULL,                            --手机
  US_eMail VARCHAR(100) NULL,                           --邮箱
  US_RegAlias VARCHAR(100) NULL,                        --注册昵称
  US_RegTime DATE NOT NULL,                             --注册时间
  US_Password VARCHAR(500) NOT NULL,                    --登录口令(散列值)
  US_DynamicPassword VARCHAR(500) NOT NULL,             --动态口令(散列值)
  US_DynamicPasswordCreateTime DATE NOT NULL,           --动态口令生成时间
  US_RequestAddress VARCHAR(39) NOT NULL,               --服务请求方IP地址
  US_RequestFailureCount NUMERIC(2) default 0 NOT NULL, --服务请求失败次数
  US_RequestFailureTime DATE NULL,                      --服务请求失败时间
  US_Root_Teams_ID NUMERIC(15) NULL,                    --所属顶层团体
  US_Teams_ID NUMERIC(15) NULL,                         --所属团体
  US_Position_ID NUMERIC(15) NULL,                      --担任岗位
  US_Locked NUMERIC(1) default 0 NOT NULL,              --是否锁定
  US_LockedTime DATE NULL,                              --锁定时间
  US_Disabled NUMERIC(1) default 0 NOT NULL,            --是否注销
  US_DisabledTime DATE NULL,                            --注销时间
  PRIMARY KEY(US_ID),
  UNIQUE(US_Name)
)
/

