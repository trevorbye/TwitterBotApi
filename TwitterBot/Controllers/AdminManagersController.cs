using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TwitterBot.Models;
using System.Security.Claims;
using TwitterBot.POCOS;

namespace TwitterBot.Controllers
{
    [Authorize]
    public class AdminManagersController : ApiController
    {
        private TwitterBotContext db = new TwitterBotContext();

        [Route("current-principal-admin")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult IsCurrentPrincipalAdmin()
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            bool isAdmin = Utilities.IsAdmin(claims, db);
            return Ok(isAdmin);
        }

        // GET: api/AdminManagers
        public IHttpActionResult GetAdminManagers()
        {
            IList<AdminManager> adminManagers = db.AdminManagers.ToList();
            if (adminManagers.Count == 0)
            {
                return NotFound();
            }
            else
            {
                return Ok(adminManagers);
            }
        }

        // GET: api/AdminManagers/5
        [ResponseType(typeof(AdminManager))]
        public async Task<IHttpActionResult> GetAdminManager(int id)
        {
            AdminManager adminManager = await db.AdminManagers.FindAsync(id);
            if (adminManager == null)
            {
                return NotFound();
            }

            return Ok(adminManager);
        }

        // PUT: api/AdminManagers/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutAdminManager(int id, AdminManager adminManager)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != adminManager.Id)
            {
                return BadRequest();
            }

            db.Entry(adminManager).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminManagerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/AdminManagers
        [ResponseType(typeof(AdminManager))]
        public async Task<IHttpActionResult> PostAdminManager(AdminManager adminManager)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.AdminManagers.Add(adminManager);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = adminManager.Id }, adminManager);
        }

        // DELETE: api/AdminManagers/5
        [ResponseType(typeof(AdminManager))]
        public async Task<IHttpActionResult> DeleteAdminManager(int id)
        {
            AdminManager adminManager = await db.AdminManagers.FindAsync(id);
            if (adminManager == null)
            {
                return NotFound();
            }

            db.AdminManagers.Remove(adminManager);
            await db.SaveChangesAsync();

            return Ok(adminManager);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool AdminManagerExists(int id)
        {
            return db.AdminManagers.Count(e => e.Id == id) > 0;
        }
    }
}