import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { CookieService } from 'ngx-cookie';

@Injectable({
  providedIn: 'root'
})
export class OnlyLoggedInUsersGuardService implements CanActivate {

  constructor(private cookie: CookieService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {

    var cok = this.cookie.get('EMBApiToken');
  
    if(cok) {
      return true;
    } else {

      this.router.navigate(['/sign-in'], {
        queryParams: {
          return: state.url
        }
      });
      return false;
    }
  }
}
