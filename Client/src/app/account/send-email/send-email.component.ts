import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from 'src/app/shared/models/account/UserDto';
import { SharedService } from 'src/app/shared/shared.service';

@Component({
  selector: 'app-send-email',
  templateUrl: './send-email.component.html',
  styleUrls: ['./send-email.component.css']
})
export class SendEmailComponent implements OnInit{


  emailForm: FormGroup = new FormGroup({

  })
  submitted = false;
  mode: string | undefined;
  errorMessages: string[] = [];

  constructor(private sharedService: SharedService, private accountService: AccountService, private formBuilder: FormBuilder, private router: Router, private activateRoute: ActivatedRoute){

  }

  ngOnInit(): void {
    this.accountService.$user.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user){
          this.router.navigateByUrl("/")
        } else {
          const mode = this.activateRoute.snapshot.paramMap.get("mode");
          
          if (mode) {
            this.mode = mode;
            this.initializeForm();
          }
        }
      }
    })
  }

  sendEmail(){
    this.submitted = true;
    this.errorMessages = [];

    if (this.emailForm.valid && this.mode){
      if (this.mode.includes(`resend-email-confirmation-link`)) {
        this.accountService.resendEmailConfirmationLink(this.emailForm.get("email")?.value).subscribe({
          next: (res: any) => {
            this.sharedService.showNotification(true, res.value.title, res.value.message);
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
      } else if(this.mode.includes("forgot-username-or-password")){
        this.accountService.forgotUsernameOrPassword(this.emailForm.get("email")?.value).subscribe({
          next: (res: any) => {
            this.sharedService.showNotification(true, res.value.title, res.value.message);
            this.router.navigateByUrl("/account/;ogin")
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
  }

  cancel() {
    this.emailForm.reset()
    this.router.navigateByUrl("/")
  }

  initializeForm() {
    this.emailForm = this.formBuilder.group({
      email: ["", [Validators.required]],
    })
  }
}
