using System;
using Microsoft.EntityFrameworkCore;
using PROG7311.Data;

namespace PROG7311.Tests
{
    public static class TestHelpers
    {
        public static ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}