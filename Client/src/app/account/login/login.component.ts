import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SharedService } from 'src/app/shared/shared.service';
import { AccountService } from '../account.service';
import { take } from 'rxjs';
import { User } from 'src/app/shared/models/UserDto';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent  implements OnInit{

  loginForm: FormGroup = new FormGroup({});
  submitted = false;
    errorMessages: string[] = [];
  returnUrl: string | null = null;

    constructor(private route: ActivatedRoute, private sharedService: SharedService, private accountService: AccountService, private formBuilder: FormBuilder, private router: Router) {
      this.accountService.$user.pipe(take(1)).subscribe({
        next: (user: User | null) => {
          if (user){
            this.router.navigateByUrl("/")
          } else {
            this.route.queryParamMap.subscribe({
              next: (params) => {
                this.returnUrl = params.get('')
              }
            })
          }
        },
        error: ()=> {

        }
      })
    }
  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(){
    this.loginForm = this.formBuilder.group({
      userName: ["", [Validators.required]],
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
          if (this.returnUrl){
            this.router.navigateByUrl(this.returnUrl)
          }
          //this.sharedService.showNotification(true, res.valueOf.title, res.valueOf.message)
          this.router.navigateByUrl("/");
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
