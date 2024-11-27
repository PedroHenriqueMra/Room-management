// using System.Security.Claims;

// namespace MinimalApi.Middleware;

// public class MyMiddleware
// {
//     private readonly RequestDelegate _next;

//     public MyMiddleware(RequestDelegate next)
//     {
//         _next = next;
//     }

//     public async Task InvokeAsync(HttpContext context)
//     {
//         // Verifica se o usuário está autenticado
//         if (context.User.Identity.IsAuthenticated)
//         {
//             // Acessa as claims do usuário
//             var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//             var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;

//             // Faça algo com as claims...
//         }

//         // Continua o processamento da requisição
//         await _next(context);
//     }
// }
