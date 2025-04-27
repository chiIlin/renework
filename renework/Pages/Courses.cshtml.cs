using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;
using System.Collections.Generic;

namespace renework.Pages
{
    public class CoursesModel : PageModel
    {
        private readonly ICourseRepository _courses;

        public CoursesModel(ICourseRepository courses)
            => _courses = courses;

        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            // load all courses
            Courses = (await _courses.GetAllAsync()).ToList();
        }
    }
}
