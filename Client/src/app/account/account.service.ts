import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Register} from "../shared/models/account/Register";
import {environment} from "../../environments/environment.development";
import { Login } from '../shared/models/account/login';
import { User } from '../shared/models/account/UserDto';
import { map, of, ReplaySubject } from 'rxjs';
import { Router } from '@angular/router';
import { ConfirmEmail } from '../shared/models/account/confirmEmail';
import { ResetPassword } from '../shared/models/account/ResetPassword';
import { RegisterWithExternal } from '../shared/models/account/RegisterWithExternal';
import { LoginWithExternal } from '../shared/models/account/LoginWithExternal';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
 
  private userSource = new ReplaySubject<User | null>(1);
  $user = this.userSource.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.$user.subscribe({
          next: (user) => {
            console.log(user)
          }
        })
   }

  register(model: Register){
    return this.http.post(`${environment.appUrl}api/account/register`, model);
  }

  refreshUser(jwt: string | null){
    if (jwt === null){
      this.userSource.next(null)
      return of(undefined)
    }

    let headers = new HttpHeaders();
    headers = headers.set("Authorization", "Bearer " + jwt);
    return this.http.get<User>(`${environment.appUrl}/api/account/refresh-user-token`, {headers}).pipe(
      map((user: User) => {
        if (user){
          this.setUser(user)
        }
      })
    );
  }

  confirmEmail(confirmEmail: ConfirmEmail) {
    return this.http.put(`${environment.appUrl}/api/account/confirm-email`, confirmEmail)
  }

  login(model: Login) {
    return this.http.post<any>(`${environment.appUrl}api/account/login`, model).pipe(
      map((user: User) => {
        if (user){
          this.setUser(user)
          return user;
        }
        return null;
      }));
  }

  logout() {
    localStorage.removeItem(environment.userKey);
    this.userSource.next(null);
    this.router.navigateByUrl("/")
  }

  LoginWithThirdParty(model: LoginWithExternal) {
    return this.http.post<User>(`${environment.appUrl}/api/account/login-with-third-party`, model).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
        }
      })
    )
  }

  registerWithThirdParty(model: RegisterWithExternal) {
    return this.http.post<User>(`${environment.appUrl}/api/account/register-with-third-party`, model).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
        }
      })
    );
  }
  
  resendEmailConfirmationLink(email: string){
    return this.http.post(`${environment.appUrl}/api/account/resend-email-confirmation-link/${email}`, {})
  }

  resetPassword(model: ResetPassword) {
    return this.http.put(`${environment.appUrl}/api/account/reset-password`, model)
  }


  forgotUsernameOrPassword(email: string) {
    return this.http.post(`${environment.appUrl}/api/account/forgot-username-or-password/${email}`, {})
  }

  getJWT(){
    const key = localStorage.getItem(environment.userKey);

    if (key){
      const user: User = JSON.parse(key)
      return user.jwt;
    } 

    return null;
  }

  private setUser(user: User){
    localStorage.setItem(environment.userKey, JSON.stringify(user))
    this.userSource.next(user);
  }
}
