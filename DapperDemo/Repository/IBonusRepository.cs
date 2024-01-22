using DapperDemo.Models;

namespace DapperDemo.Repository
{
    public interface IBonusRepository
    {
        List<Employee> GetEmployeeWithCompany(int id);
        Company GetCompanyWithEmployees(int id);
        List<Company> GetAllCompaniesWithEmployees();
        void AddTestCompanyWithEmployees(Company objComp);
        void RemoveRange(int[] companyId);
        List<Company> FilterCompanyByName(string name);
        void AddTestCompanyWithEmployeesWithTransaction(Company objComp);
    }
}
