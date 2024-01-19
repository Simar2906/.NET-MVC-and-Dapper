using Dapper;
using DapperDemo.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DapperDemo.Repository
{
    public class BonusRepository:IBonusRepository
    {
        private IDbConnection db;

        public BonusRepository(IConfiguration configuration)
        {
            this.db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        public List<Employee> GetEmployeeWithCompany(int id)
        {
            var sql = "SELECT E.*,C.* FROM Employees AS E INNER JOIN Companies AS C ON E.CompanyId = C.CompanyId";
            if(id!= 0)
            {
                sql += " WHERE E.CompanyId = @Id";
            }
            
            var employee = db.Query<Employee, Company, Employee>(sql, (empl, comp) =>
            {
                empl.Company = comp;
                return empl;
            }, new {id}, splitOn:"CompanyId");

            return employee.ToList();
        }
        List<Company> GetAllCompaniesWithEmployees()
        {
            var sql = "SELECT E.*,C.* FROM Employees AS E INNER JOIN Companies AS C ON E.CompanyId = C.CompanyId";
            var companyDic = new Dictionary<int, Company>();

            var company = db.Query<Company, Employee, Company>(sql, (c, e) =>
            {
                if (!companyDic.TryGetValue(c.CompanyId, out var currentCompany))
                {
                    currentCompany = c;
                    companyDic.Add(currentCompany.CompanyId, currentCompany);
                }
                currentCompany.Employees.Add(e);
                return currentCompany;
            }, splitOn:"EmployeeId") ;
            return company.Distinct().ToList();
        }
        public Company GetCompanyWithEmployees(int id)
        {
            var p = new
            {
                CompanyId = id
            };
            var sql = "SELECT * FROM COMPANIES WHERE CompanyId = @CompanyId;"
                + " SELECT * FROM Employees WHERE CompanyId = @CompanyId;";

            Company company;
            using (var lists = db.QueryMultiple(sql, p))
            {
                company = lists.Read<Company>().ToList().FirstOrDefault();
                company.Employees = lists.Read<Employee>().ToList();
            }
            return company;
        }
    }
}
