import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SharedService } from 'src/app/shared/shared.service';
import { AccountService } from '../account.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent  implements OnInit{

  loginForm: FormGroup = new FormGroup({});
  submitted = false;
    errorMessages: string[] = [];

    constructor(private sharedService: SharedService, private accountService: AccountService, private formBuilder: FormBuilder, private router: Router) {
    }
  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(){
    this.loginForm = this.formBuilder.group({
      username: ["", [Validators.required]],
      password: ["", [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    })
  }

  login(){
    this.submitted = true;
    this.errorMessages = [];

     if (this.loginForm.valid) {
      this.accountService.login(this.loginForm.value).subscribe({
        next:(res: any)=> {
          console.log(res);
          this.sharedService.showNotification(true, res.valueOf.title, res.valueOf.message)
          this.router.navigateByUrl("/account/login");
        },
        complete:() => {},
        error:(err)=> {
          console.log(err)
          if (err.error.errors){
            this.errorMessages = err.error.errors;
          } else {
            this.errorMessages.push(err.error)
          }
          //this.errorMessages.push()
        }
      })
    }
  }

}
