import { inject, Injectable } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})

class AuthGuardService{
  constructor(private router:Router, private authService:AuthService){}

  canActivate():boolean{
    if(this.authService.isLoggedIn()){
      return true;
    }else{
      this.router.navigate(['/auth/login'], {
        queryParams: { returnUrl: this.router.url }
      });
      return false;
    }
  }
}
export const authGuard: CanActivateFn = (route, state) => {
  return inject(AuthGuardService).canActivate();
};
