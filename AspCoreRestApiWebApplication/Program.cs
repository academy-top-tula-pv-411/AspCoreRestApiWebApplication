using AspCoreRestApiWebApplication;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<Employee> employees = new()
{
    new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Bobby",
        Age = 28
    },
    new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Sammy",
        Age = 31
    },
    new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Jimmy",
        Age = 24
    },
};

app.Run(async context =>
{
    var request = context.Request;
    var response = context.Response;
    
    var path = request.Path;
    var method = request.Method;

    string patternGuid = @"^/api/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

    if (path == "/api" && request.Method == "GET")
    {
        await response.WriteAsJsonAsync(employees);
    }

    else if (Regex.IsMatch(path, patternGuid) && method == "GET")
    {
        string id = path.Value.Substring(path.Value.LastIndexOf('/') + 1);

        Employee employee = employees.FirstOrDefault(e => e.Id == id);
        if (employee is not null)
            await response.WriteAsJsonAsync(employee);
        else
        {
            response.StatusCode = 404;
            await response.WriteAsJsonAsync(new { message = "Employee not found" });
        }
    }

    else if (path == "/api" && method == "POST")
    {
        try
        {
            var employee = await request.ReadFromJsonAsync<Employee>();

            if (employee is not null)
            {
                employee.Id = Guid.NewGuid().ToString();
                employees.Add(employee);

                await response.WriteAsJsonAsync(employee); // ???
            }
            else
                throw new Exception("Incorrect data");
        }
        catch (Exception ex)
        {
            response.StatusCode = 404;
            await response.WriteAsJsonAsync(new { message = ex.Message });
        }
    }

    else if (path == "/api" && method == "PUT")
    {
        try
        {
            Employee? employeeClient = await request.ReadFromJsonAsync<Employee>();
            if (employeeClient is not null)
            {
                var employee = employees.FirstOrDefault(e => e.Id == employeeClient.Id);
                if (employee is not null)
                {
                    employee.Name = employeeClient.Name;
                    employee.Age = employeeClient.Age;
                    await response.WriteAsJsonAsync(employee);
                }
                else
                {
                    throw new Exception("Employee not found");
                }
            }
            else
            {
                throw new Exception("Incorrect data");
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = 404;
            await response.WriteAsJsonAsync(new { message = ex.Message });
        }
    }

    else if (Regex.IsMatch(path, patternGuid) && method == "DELETE")
    {
        string id = path.Value.Substring(path.Value.LastIndexOf('/') + 1);

        var employee = employees.FirstOrDefault(e => e.Id == id);
        if (employee is not null)
        {
            employees.Remove(employee);
            await response.WriteAsJsonAsync(employee);
        }
        else
        {
            response.StatusCode = 404;
            await response.WriteAsJsonAsync(new { message = "Employee not found" });
        }
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("index.html");
    }

    
});

app.Run();
