// ---- 服務命名空間（一般 using）----
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections; // for HttpTransportType
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;           // for AddSignalR
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;            // CookieEvents 內有用到 .Any()
using System.Threading.Tasks; // async Main

// ---- 社群 Hub / 過濾器 / 共用登入 ----
using GameSpace.Areas.social_hub.Hubs;
using GameSpace.Infrastructure.Login;

// ---- MiniGame Area ----
using GameSpace.Areas.MiniGame.Services;


// ---- 型別別名（避免撞名）----
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;
using INotificationServiceAlias = GameSpace.Areas.social_hub.Services.INotificationService;
using ManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Services.ManagerPermissionService;

using MuteFilterAlias = GameSpace.Areas.social_hub.Services.MuteFilter;
using NotificationServiceAlias = GameSpace.Areas.social_hub.Services.NotificationService;
