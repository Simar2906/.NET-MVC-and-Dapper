using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Runtime.Intrinsics.Arm;
using Dapper;
using DapperDemo.Data;
using DapperDemo.Models;
using Microsoft.Data.SqlClient;

namespace DapperDemo.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private IDbConnection db;

        public EmployeeRepository(IConfiguration configuration)
        {
            this.db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }
        public Employee Add(Employee employee)
        {
            var sql = "INSERT INTO Employee(Name, Title, Email, Phone, CompanyId) VALUES(@Name, @Title, @Email, @Phone, @CompanyId); "
                        + "SELECT CAST(SCOPE_IDENTITY() as int); ";
            var id = db.Query<int>(sql, employee).Single();
            employee.EmployeeId = id;
            return employee;
        }

        public Employee Find(int id)
        {
            var sql = $"SELECT * FROM Employee WHERE EmployeeId= @EmployeeId";
            return db.Query<Employee>(sql, new {@EmployeeId = id}).Single();
        }

        public List<Employee> GetAll()
        {
            var sql = "SELECT * FROM Employee";
            return db.Query<Employee>(sql).ToList();
        }

        public void Remove(int id)
        {
            var sql = "DELETE FROM Employee WHERE EmployeeId = @id";
            db.Execute(sql, new {id});
        }

        public Employee Update(Employee company)
        {
            var sql = "UPDATE Employee SET Name = @Name, Title = @Title, Email = @Email, Phone = @Phone, CompanyId = @CompanyId WHERE EmployeeId = @EmployeeId";

            db.Execute(sql, company);
            return company;
        }
    }
}
