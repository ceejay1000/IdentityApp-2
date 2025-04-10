import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Register} from "../shared/models/Register";
import {environment} from "../../environments/environment.development";
import { Login } from '../shared/models/login';
import { User } from '../shared/models/UserDto';
import { map, of, ReplaySubject } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
 
 
  private userSource = new ReplaySubject<User | null>(1);
  $user = this.userSource.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

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
