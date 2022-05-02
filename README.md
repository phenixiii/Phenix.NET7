Phenix.NET7 是一款基于.net6的企业级分布式应用软件开发框架。
本框架重写了前版的持久层引擎，可支持多库操作，对实体的LINQ操作会自动转化对数据库（Oracle、SQLServer、MySQL、PostgreSQL）透明化的CRDU操作，可规范开发人员的编码，简洁、可读性强，执行效率也更加高效。本框架还封装了Orleans框架，支持高并发、分布式服务的技术场景，也充分应用了OOP、DDD设计思想，尽最大努力让开发人员把精力花在业务分析和设计开发上，而不是具体的技术实现细节上，从而大大缩短开发周期，降低开发成本，保证系统的稳定性、可维护性。
作为使用框架的示例项目，Phenix TPT v2 是一款项目跟踪管理工具（Teamwork Project Tracker），目前仅实现了v1的核心功能（项目日志、工作量填报），后续会完善开票和支出费用的流程管控、项目评价体系、研发评价体系等功能。