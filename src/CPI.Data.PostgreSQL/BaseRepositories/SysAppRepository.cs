using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Models;
using CPI.IData.BaseRepositories;

namespace CPI.Data.PostgreSQL.BaseRepositories
{
    public class SysAppRepository : EFRepository<SysApp>, ISysAppRepository
    {
    }
}
