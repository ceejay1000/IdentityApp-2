import {Component, OnInit} from '@angular/core';
import {AccountService} from "../account.service";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {Router} from "@angular/router";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  registerForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages: string[] = [];
  constructor(private accountService: AccountService, private formBuilder: FormBuilder, private router: Router) {
  }
  ngOnInit(): void {
    this.initializeForm();
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

    if (this.registerForm.valid) {
      this.accountService.register(this.registerForm.value).subscribe({
        next:(res)=> {
          console.log(res);
        },
        complete:() => {},
        error:(err)=> {
          console.log(err)
          //this.errorMessages.push()
        }
      })
    }
    console.log(this.registerForm.value);
  }
}
