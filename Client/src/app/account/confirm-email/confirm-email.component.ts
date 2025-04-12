import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account.service';
import { SharedService } from 'src/app/shared/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from 'src/app/shared/models/account/UserDto';
import { ConfirmEmail } from 'src/app/shared/models/account/confirmEmail';

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.css']
})
export class ConfirmEmailComponent implements OnInit{

  success: boolean = false;


  constructor(private accountService: AccountService, private sharedService: SharedService, private router: Router, private activatedRoute: ActivatedRoute){

  }

  ngOnInit(): void {
    this.accountService.$user.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          this.router.navigateByUrl("/")
        } else {
          this.activatedRoute.queryParamMap.subscribe({
            next: (params: any) => {
              const confirmEmail: ConfirmEmail = {
                email : params.get("email"),
                token : params.get("token")
              }

              this.accountService.confirmEmail(confirmEmail).subscribe({
                next: (res: any) => {
                  this.sharedService.showNotification(true, res.value.title, res.value.message);
                },
                error: (err) => {
                  this.success = false
                  this.sharedService.showNotification(false, "failed", err.error)
                }
              })
            }
          })
        }
      }
    })
  }

  resendConfirmationLink() {
    this.router.navigateByUrl("account/send-email/resend-email-confirmation-link")
  }

}
