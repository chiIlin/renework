// Pages/Courses.cshtml.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.Repositories.Interfaces;
using renework.MongoDB.Collections;

namespace renework.Pages
{
    public class CoursesModel : PageModel
    {
        private readonly ICourseRepository _courses;

        public CoursesModel(ICourseRepository courses)
        {
            _courses = courses;
        }

        // exposed to the .cshtml
        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            // load all courses
            Courses = (await _courses.GetAllAsync()).ToList();
        }
    }
}
