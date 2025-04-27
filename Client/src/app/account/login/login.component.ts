import { Component, ElementRef, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SharedService } from 'src/app/shared/shared.service';
import { AccountService } from '../account.service';
import { take } from 'rxjs';
import { User } from 'src/app/shared/models/account/UserDto';
import { DOCUMENT } from '@angular/common';
import { CredentialResponse } from 'google-one-tap';
import { jwtDecode } from 'jwt-decode';
import { LoginWithExternal } from 'src/app/shared/models/account/LoginWithExternal';

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
    @ViewChild("googleButton", {static: true}) googleButton: ElementRef = new ElementRef({});
  

    constructor(@Inject(DOCUMENT) private _document: Document, private route: ActivatedRoute, private sharedService: SharedService, private accountService: AccountService, private formBuilder: FormBuilder, private router: Router) {
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
    this.initializeGoogleButton();
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

    private initializeGoogleButton() {
      (window as any).onGoogleLibraryLoad = () => {
        // @ts-ignore
        google.accounts.id.initialize({client_id: "22201231380-ju5vg2vmdmkklf6n08k0mopcii62j4up.apps.googleusercontent.com",
          callback: this.googleCallback.bind(this),
          auto_select: false,
          cancel_on_tap_outside: true
        });
  
        // @ts-ignore
        google.accounts.id.renderButton(
          this.googleButton.nativeElement,
          {size: "medium", shape: "rectangular", text: "signin_with", logo_alignment: "center"}
        )
      }
    }
  
    private async googleCallback(response: CredentialResponse){
      console.log(response)
      const decodedToken: any = jwtDecode(response.credential);
  //    this.router.navigateByUrl(`/account/register.third-party/google/access_token=${response.credential}&userId=${decodedToken.sub}`)
      this.accountService.LoginWithThirdParty(new LoginWithExternal(response.credential, decodedToken.sub, "google"))
      .subscribe({
        next: _ => {
          if (this.returnUrl){
            this.router.navigateByUrl(this.returnUrl)
          } else {
            this.router.navigateByUrl("/")
          }
        }, 
        error: error => {
          this.sharedService.showNotification(false, "failed", error.error)
        }
      })
  
    }

  resendEmailConfirmationLink() {
    this.router.navigateByUrl("account/send-email/resend-email-confirmation-link");
  }

}
