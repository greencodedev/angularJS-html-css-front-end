import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit {
  User:any = { Email:""};
  

  constructor(private api: ApiService, private router: Router) { }

  ngOnInit() {
  }

  ForgetPassword(){
    var $this = this;
    this.api.ForgetPassword(this.User,
      data => { 
    alert("Please check your email on how to reset your password");
    setTimeout(() => {
      this.router.navigate(['sign-in']);
  }, 2000);  //5s
  },
    error => {
      alert(error.Messages.join(','));
      }
  )}

}
