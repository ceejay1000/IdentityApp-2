import {Component, ElementRef, Inject, inject, OnInit, Renderer2, ViewChild} from '@angular/core';
import {AccountService} from "../account.service";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {Router} from "@angular/router";
import { SharedService } from 'src/app/shared/shared.service';
import { take } from 'rxjs';
import { User } from 'src/app/shared/models/account/UserDto';
import { environment } from 'src/environments/environment.development';
import { CredentialResponse } from 'google-one-tap';
import { jwtDecode } from 'jwt-decode';
import { DOCUMENT } from '@angular/common';
// import * as google from 'google-one-tap';//

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  registerForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages: string[] = [];
  @ViewChild("googleButton", {static: true}) googleButton: ElementRef = new ElementRef({});

  constructor(@Inject(DOCUMENT) private _document: Document,private renderer2: Renderer2, private sharedService: SharedService, private accountService: AccountService, private formBuilder: FormBuilder, private router: Router) {
    this.accountService.$user.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user){
          this.router.navigateByUrl("/")
        }
      }
    })
  }
  ngOnInit(): void {
    this.initializeForm();
    this.initializeGoogleButton()
  }

  ngAfterViewInit(){
    const script = this.renderer2.createElement("script");
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    this.renderer2.appendChild(this._document.body, script)
  }

  // registerWithFacebook() {
  //   FB.login(async (fbResult: any) => {
  //     if (fbResult.authResponse) {
  //       const accessToken = fbResult.authResponse.accessToken;
  //       const userId = fbResult.authResponse.userID;
  //       this.router.navigateByUrl(`/account/register/third-party/facebook?access_token=${accessToken}&userId=${userId}`);
  //     } else {
  //       this.sharedService.showNotification(false, "Failed", "Unable to register with your facebook");
  //     }
  //   })
  // }
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
        {size: "medium", shape: "rectangular", text: "continue_with", logo_alignment: "center"}
      )
    }
  }

  private async googleCallback(response: CredentialResponse){
    const decodedToken: any = jwtDecode(response.credential);
    this.router.navigate([`/account/register/third-party/google?access_token=${response.credential}&userId=${decodedToken.sub}`])

  }

  initializeForm(): void {
    this.registerForm = this.formBuilder.group({
      firstName: ["", [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      lastName: ["", [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      email: ["", [Validators.required]],
      password: ["", [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    })
  }

  register() {
    this.submitted = true;
    this.errorMessages = [];

    // if (this.registerForm.valid) {
      this.accountService.register(this.registerForm.value).subscribe({
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
    // }
    console.log(this.registerForm.value);
  }
}
