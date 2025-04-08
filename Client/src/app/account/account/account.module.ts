import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountComponent } from './account.component';
import {RouterModule} from "@angular/router";
import {AccountRoutingModule} from "../account-routing.module";
import {SharedModule} from "../../shared/shared.module";



@NgModule({
  declarations: [
    AccountComponent
  ],
  imports: [
    CommonModule,
    AccountRoutingModule,
    SharedModule,
  ]
})
export class AccountModule { }
