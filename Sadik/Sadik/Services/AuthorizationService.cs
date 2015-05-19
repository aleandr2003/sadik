using Sadik.Models;
using Sadik.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public bool Authorize(Operation op, object obj)
        {
            switch (op)
            {
                case Operation.EditUser:
                    {
                        var u = obj as User;
                        return curUser != null && u != null && u.Id == curUser.Id;
                    }
                case Operation.ManageInvetoryItems:
                    {
                        if (curUser == null || curUser.RoleId != UserRole.Teacher)
                            return false;
                        if (obj is Inventory)
                        {
                            var item = obj as Inventory;
                            if (KindergartenIds.Contains(item.KindergartenId)) return true;
                        }
                        else if (obj is Zone)
                        {
                            var zone = obj as Zone;
                            if (KindergartenIds.Contains(zone.KindergartenId)) return true;
                        }
                        return false;
                    }
                case Operation.ManageObservationNotes:
                    {
                        if (curUser == null || curUser.RoleId != UserRole.Teacher)
                            return false;
                        var kid = (Kid)obj;
                        if (!KindergartenIds.Contains(kid.KindergartenId)) return false;
                        return true;
                    }
                case Operation.AddObsevations:
                    {
                        if (curUser == null || curUser.RoleId != UserRole.Teacher)
                            return false;
                        var kid = (Kid)obj;
                        if (!KindergartenIds.Contains(kid.KindergartenId)) return false;
                        return true;
                    }
                case Operation.ManageKids:
                    {
                        if (curUser == null || curUser.RoleId != UserRole.Teacher)
                            return false;
                        var kid = (Kid)obj;
                        if (!KindergartenIds.Contains(kid.KindergartenId)) return false;
                        return true;
                    }
                case Operation.ManageADRs:
                    {
                        if (curUser == null || curUser.RoleId != UserRole.Teacher)
                            return false;
                        var kid = (Kid)obj;
                        if (!KindergartenIds.Contains(kid.KindergartenId)) return false;
                        return true;
                    }
                case Operation.RemoveUsers:
                    return curUser != null && curUser.RoleId == UserRole.Admin;
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }

        public bool Authorize(Operation op)
        {
            switch (op)
            {
                case Operation.CreateUsers:
                    return true;//curUser != null && curUser.RoleId == UserRole.Admin;
                case Operation.RemoveUsers:
                    return curUser != null && curUser.RoleId == UserRole.Admin;
                case Operation.CreateKindergartens:
                    return curUser != null && curUser.RoleId == UserRole.Admin;
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }

        public bool Authorize(Operation op, int KindergartenId)
        {
            switch (op)
            {
                case Operation.AddInvetoryItems:
                case Operation.AcceptKids:
                case Operation.ManageKids:
                case Operation.AddObsevations:
                    {
                        if (curUser == null || curUser.RoleId != UserRole.Teacher)
                            return false;
                        if (!KindergartenIds.Contains(KindergartenId))
                            return false;
                        return true;
                    }
                case Operation.ManageEmployees:
                case Operation.ManageKindergarten:
                    {
                        if (curUser == null)
                            return false;
                        if (curUser.RoleId == UserRole.Admin)
                            return true;
                        if (curUser.RoleId == UserRole.Teacher && !KindergartenIds.Contains(KindergartenId))
                            return false;
                        return true;
                    }
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }

        public AuthorizationService(IUserSession userSession)
        {
            curUser = userSession.IsAuthenticated ? userSession.CurrentUser : null;
            KindergartenIds = userSession.KindergartenIds;
        }

        private readonly User curUser;
        private readonly List<int> KindergartenIds;
    }
}