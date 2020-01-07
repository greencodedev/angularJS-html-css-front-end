import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { CookieModule } from 'ngx-cookie';
import { AppRoutingModule } from './app-routing/app-routing.module';
import { AppComponent } from './app.component';
import { SignInComponent } from './Components/auth/signIn/signIn.component';
import { MainComponent } from './Components/main/main.component';
import { ProductsComponent } from './products/products.component';
import {
  BsDropdownModule,
  PopoverModule,
  CollapseModule,
  ModalModule,
  PaginationModule,
  TabsModule,
  TypeaheadModule
} from 'ngx-bootstrap';
import { HeaderComponent } from './Components/header/header.component';
import { ModalComponent } from './Components/modal/modal.component';
import { ForgotPasswordComponent } from './Components/auth/forgot-password/forgot-password.component';
import { PreferencesModule } from './preferences/preferences.module';
import { ResetPasswordComponent } from './Components/reset-password/reset-password.component';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { ConvertModalComponent } from './Components/convert-modal/convert-modal.component';


@NgModule({
  declarations: [
    AppComponent,
    SignInComponent,
    MainComponent,
    ProductsComponent,
    HeaderComponent,
    ModalComponent,
    ConvertModalComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,   
  ],

  imports: [
    FormsModule,
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    CookieModule.forRoot(),
    BsDropdownModule.forRoot(),
    FormsModule,
    HttpClientModule,
    BsDropdownModule.forRoot(),
    PopoverModule.forRoot(),
    ModalModule.forRoot(),
    PaginationModule.forRoot(),
    PreferencesModule,
    TooltipModule.forRoot(),
    TabsModule.forRoot(),
    TypeaheadModule.forRoot(),
    CollapseModule
  ],
  providers: [],
  bootstrap: [AppComponent],
  entryComponents: [
    ModalComponent,
    ConvertModalComponent
  ]
})
export class AppModule {
}
