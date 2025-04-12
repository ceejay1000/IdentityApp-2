import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SharedService } from 'src/app/shared/shared.service';
import { take } from 'rxjs';
import { User } from 'src/app/shared/models/account/UserDto';
import { ResetPassword } from 'src/app/shared/models/account/ResetPassword';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {

  email: any;
  token: any;

  resetPasswordForm: FormGroup = new FormGroup({})
  submitted: boolean = false;
  errorMessages: any[] = [];

  constructor(private sharedService: SharedService, private accountService: AccountService, private formBuilder: FormBuilder, private router: Router, private activateRoute: ActivatedRoute){}

  ngOnInit(): void {
    this.accountService.$user.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) this.router.navigateByUrl("/")
        else this.activateRoute.queryParamMap.subscribe({
          next:(params: any) => {
            this.token = params.get("token")
            this.email = params.get("email")

            if (this.token && this.email) this.initializeForm(this.email);
            else this.router.navigateByUrl("/account/login")
          }
        })
      }
    })
  } 

  resetPassword() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.resetPasswordForm.valid && this.email && this.token){
      const model: ResetPassword = {
        token: this.token,
        email: this.email,
        newPassword: this.resetPasswordForm.get("newPassword")?.value
      };

      this.accountService.resetPassword(model).subscribe({
        next: (res: any) => {
          this.sharedService.showNotification(true, res.value.title, res.vaue.message);
          this.router.navigateByUrl("/account/login")
        },
        error: (err) => {
          if (err.error.errors){
            this.errorMessages = err.error.errors;
          } else {
            this.errorMessages.push(err.error)
          }
        }
      })
    }
  }

  cancel() {
    this.router.navigateByUrl("/")
  }

  initializeForm(username: string){
    this.resetPasswordForm = this.formBuilder.group({
      email: [{value: username, disabled: true}],
      newPassword: ["", [Validators.required, Validators.minLength(6), Validators.maxLength(15)]]
    })
  }

}
