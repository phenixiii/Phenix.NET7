{*******************************************************************************
 *
 * Unit Name : Net.sql
 * Purpose   : Net
 * Author    : ming
 * Maintainer:
 * History   :
 *
 * CreateDate: 2019-07-18
 *******************************************************************************}

CREATE TABLE PH7_Controller_Role (         --控制器的授权角色
  CR_ID NUMERIC(15) NOT NULL,
  CR_ControllerName VARCHAR(255) NOT NULL, --Controller全名
  CR_ActionName VARCHAR(255) NOT NULL,     --Action名
  CR_Roles VARCHAR(4000) NULL,             --角色数组(存储的是[Authorize]标签 Roles 属性值，如有多个[Authorize]就用','分隔它们，字段为 null 时等同于[AllowAnonymous])
  PRIMARY KEY(CR_ID),
  UNIQUE(CR_ControllerName, CR_ActionName)
)
/