using Microsoft.AspNetCore.Mvc;
using TuneScout.Models;
using Logic.Services;
using Logic.Models;
using System.Linq;

namespace TuneScout.Controllers
{
    //public class UserController : Controller
    //{
    //    private readonly UserService _userService;

    //    public UserController(UserService userService)
    //    {
    //        _userService = userService;
    //    }

    //    // GET: UserController
    //    public ActionResult Index()
    //    {
    //        var users = _userService.GetAllUsers()
    //            .Select(u => new UserViewModel
    //            {
    //                Id = u.Id,
    //                Name = u.Name,
    //                Email = u.Email,
    //                Password = u.Password,
    //                NoExplicit = u.NoExplicit
    //            }).ToList();
    //        return View(users);
    //    }

    //    // GET: UserController/Details/5
    //    public ActionResult Details(int id)
    //    {
    //        var user = _userService.GetUserById(id);
    //        if (user == null) return NotFound();
    //        var vm = new UserViewModel
    //        {
    //            Id = user.Id,
    //            Name = user.Name,
    //            Email = user.Email,
    //            Password = user.Password,
    //            NoExplicit = user.NoExplicit
    //        };
    //        return View(vm);
    //    }

    //    // GET: UserController/Create
    //    public ActionResult Create() => View();

    //    // POST: UserController/Create
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create(UserViewModel vm)
    //    {
    //        if (!ModelState.IsValid) return View(vm);
    //        var user = new User
    //        {
    //            Name = vm.Name,
    //            Email = vm.Email,
    //            Password = vm.Password,
    //            NoExplicit = vm.NoExplicit
    //        };
    //        _userService.CreateUser(user);
    //        return RedirectToAction(nameof(Index));
    //    }

    //    // GET: UserController/Edit/5
    //    public ActionResult Edit(int id)
    //    {
    //        var user = _userService.GetUserById(id);
    //        if (user == null) return NotFound();
    //        var vm = new UserViewModel
    //        {
    //            Id = user.Id,
    //            Name = user.Name,
    //            Email = user.Email,
    //            Password = user.Password,
    //            NoExplicit = user.NoExplicit
    //        };
    //        return View(vm);
    //    }

    //    // POST: UserController/Edit/5
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit(int id, UserViewModel vm)
    //    {
    //        if (!ModelState.IsValid) return View(vm);
    //        var user = new User
    //        {
    //            Id = vm.Id,
    //            Name = vm.Name,
    //            Email = vm.Email,
    //            Password = vm.Password,
    //            NoExplicit = vm.NoExplicit
    //        };
    //        _userService.UpdateUser(user);
    //        return RedirectToAction(nameof(Index));
    //    }

    //    // GET: UserController/Delete/5
    //    public ActionResult Delete(int id)
    //    {
    //        var user = _userService.GetUserById(id);
    //        if (user == null) return NotFound();
    //        var vm = new UserViewModel
    //        {
    //            Id = user.Id,
    //            Name = user.Name,
    //            Email = user.Email,
    //            Password = user.Password,
    //            NoExplicit = user.NoExplicit
    //        };
    //        return View(vm);
    //    }

    //    // POST: UserController/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        _userService.DeleteUser(id);
    //        return RedirectToAction(nameof(Index));
    //    }
    //}
}