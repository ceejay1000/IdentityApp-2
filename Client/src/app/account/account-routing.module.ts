import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {RouterModule, Routes} from "@angular/router";
import {LoginComponent} from "./login/login.component";
import {RegisterComponent} from "./register/register.component";
import {NotFoundComponent} from "../shared/components/errors/not-found/not-found.component";


const routes: Routes = [
  { path: 'login', component: LoginComponent,},
  { path: 'register', component: RegisterComponent},
  {path: '**', component: NotFoundComponent},
]
@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule.forChild(routes)
  ],
  exports: [RouterModule]
})
export class AccountRoutingModule { }
