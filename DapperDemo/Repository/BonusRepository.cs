using Dapper;
using DapperDemo.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        public List<Company> GetAllCompaniesWithEmployees()
        {
            var sql = "SELECT C.*,E.* FROM Employees AS E INNER JOIN Companies AS C ON E.CompanyId = C.CompanyId";
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

        public void AddTestCompanyWithEmployees(Company objComp)
        {
            var sql = "INSERT INTO Companies(Name, Address, City, State, PostalCode) VALUES (@Name, @Address, @City, @State, @PostalCode); SELECT CAST(SCOPE_IDENTITY() as int);";
            var id = db.Query<int>(sql, objComp/*new { @name = company.Name, @add = company.Address, @city = company.City, @state = company.State, @postal = company.PostalCode }*/).Single();
            objComp.CompanyId = id;
            Console.WriteLine(objComp.Address);
            //foreach (var employee in objComp.Employees)
            //{
            //    employee.CompanyId = objComp.CompanyId;
            //    var sql1 = "INSERT INTO Employees(Name, Title, Email, Phone, CompanyId) VALUES(@Name, @Title, @Email, @Phone, @CompanyId); "
            //            + "SELECT CAST(SCOPE_IDENTITY() as int); ";
            //    db.Query<int>(sql1, employee).Single();
            //}

            objComp.Employees.Select(c => { c.CompanyId = id;return c; }).ToList();
            var sqlEmp = "INSERT INTO Employees(Name, Title, Email, Phone, CompanyId) VALUES(@Name, @Title, @Email, @Phone, @CompanyId); "
                        + "SELECT CAST(SCOPE_IDENTITY() as int); ";
            db.Execute(sqlEmp, objComp.Employees);
        }
        public void AddTestCompanyWithEmployeesWithTransaction(Company objComp)
        {
            using(var transaction = new TransactionScope())
            {
                try
                {
                    var sql = "INSERT INTO Companies(Name, Address, City, State, PostalCode) VALUES (@Name, @Address, @City, @State, @PostalCode); SELECT CAST(SCOPE_IDENTITY() as int);";
                    var id = db.Query<int>(sql, objComp/*new { @name = company.Name, @add = company.Address, @city = company.City, @state = company.State, @postal = company.PostalCode }*/).Single();
                    objComp.CompanyId = id;

                    objComp.Employees.Select(c => { c.CompanyId = id; return c; }).ToList();
                    var sqlEmp = "INSERT INTO Employees(Name, Title, Email, Phone, CompanyId) VALUES(@Name, @Title, @Email, @Phone, @CompanyId); "
                                + "SELECT CAST(SCOPE_IDENTITY() as int); ";
                    db.Execute(sqlEmp, objComp.Employees);
                    transaction.Complete();
                }
                catch(Exception ex)
                {

                }
            }
            
        }
        public void RemoveRange(int[] companyId)
        {
            db.Query("DELETE FROM Companies WHERE CompanyId IN @companyId", new { companyId });
        }

        public List<Company> FilterCompanyByName(string name) {
            return db.Query<Company>("SELECT * FROM Companies WHERE Name LIKE '%'+@name+'%'", new { name }).ToList();
        }
    }
}
