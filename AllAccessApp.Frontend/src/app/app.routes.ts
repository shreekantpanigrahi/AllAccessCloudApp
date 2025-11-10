import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './auth/layout/auth-layout/auth-layout.component';
import { authGuard } from './guards/auth.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { FilesComponent } from './files/files.component';
import { ContactUsComponent } from './contact-us/contact-us.component';
import { ProfileComponent } from './profile/profile.component';

export const routes: Routes = [
    {path:'', redirectTo:'/dashboard', pathMatch:'full'},
    {path:'auth', component:AuthLayoutComponent, 
        children:[
            {path: 'login', loadComponent:()=>import('./auth/login/login.component').then(m=>m.LoginComponent)},
            {path: 'register', loadComponent:()=>import('./auth/register/register.component').then(m=>m.RegisterComponent) },
            {path: 'forgot-password', loadComponent:()=>import('./auth/forgot-password/forgot-password.component').then(m=>m.ForgotPasswordComponent)},
            {path: 'verify-otp', loadComponent:()=>import('./auth/verify-otp/verify-otp.component').then(m=>m.VerifyOtpComponent)},
            {path: 'reset-password', loadComponent:()=>import('./auth/reset-password/reset-password.component').then(m=>m.ResetPasswordComponent)},
            {path: '', redirectTo: 'login', pathMatch: 'full'}
        ]
    },
    {path:'dashboard', component: DashboardComponent},
    {path:'my-files', component: FilesComponent, canActivate: [authGuard]},
    {path: 'contact-us', component: ContactUsComponent, canActivate: [authGuard]},
    {path:'profile', component: ProfileComponent, canActivate:[authGuard]},
    {path: '**', redirectTo: '/dashboard'}
];
