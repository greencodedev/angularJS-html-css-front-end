import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SignInComponent } from '../Components/auth/signIn/signIn.component';
import { RouterModule, Routes } from '@angular/router';
import { ModalComponent } from '../Components/modal/modal.component';
import { ProductsComponent } from '../products/products.component';
import { ForgotPasswordComponent } from '../Components/auth/forgot-password/forgot-password.component';
import { logging } from 'selenium-webdriver';
import { PreferencesComponent } from '../preferences/preferences.component';
import { PrinterSetupComponent } from '../preferences/printer-setup/printer-setup.component';
import { PrintLogComponent } from '../preferences/print-log/print-log.component';
import { UsersComponent } from '../preferences/users/users.component';
import Preferences = logging.Preferences;
import { OnlyLoggedInUsersGuardService } from '../services/only-logged-in-users-guard.service';
import { ResetPasswordComponent } from '../Components/reset-password/reset-password.component';
import { LocationsComponent } from '../preferences/products/locations/locations.component';
import { ConditionsComponent } from '../preferences/products/conditions/conditions.component';
import { ConvertLogComponent } from '../preferences/convert-log/convert-log.component';
import { PrinterSettingsComponent } from '../preferences/printer-setup/printer-settings/printer-settings.component';
import { AvgCostLogComponent } from '../preferences/products/customCost/avg-cost-log/avg-cost-log.component';


const routes: Routes = [
  {
    path: '',
    redirectTo: '/products',
    pathMatch: 'full'
  },
  {
    path: 'sign-in',
    data: {title: 'Sign In'},
    component: SignInComponent
  },
  {
    path: 'forgot-password',
    data: {title: 'Forgot Password'},
    component: ForgotPasswordComponent
  },
  {
    path: 'reset-password',
    data: {title: 'rest Password'},
    component: ResetPasswordComponent
  },
  {
    path: 'products',
    data: {title: 'Products'},
    canActivate: [OnlyLoggedInUsersGuardService],
    component: ProductsComponent
  },
  {
    path: 'preferences',
    data: {title: 'Preferences'},
    canActivate: [OnlyLoggedInUsersGuardService],
    component: PreferencesComponent,
    children: [
      {
        path: '',
        redirectTo: 'locations',
        pathMatch: 'full',
      },
      {
        path: 'locations',
        data: {title: 'Preferences | Locations'},
        component: LocationsComponent,
      },
      {
        path: 'conditions',
        data: {title: 'Preferences | Conditions'},
        component: ConditionsComponent,
      },
      {
        path: 'avg-cost-log',
        data: {title: 'Preferences | Average Cost Log'},
        component: AvgCostLogComponent,
      },
      {
        path: 'printer-setup',
        data: {title: 'Preferences | Printer Setup'},
        component: PrinterSetupComponent,
      },
      {
        path: 'label-setup',
        data: {title: 'Preferences | Label Setup'},
        component: PrinterSettingsComponent,
      },
      {
        path: 'print-log',
        data: {title: 'Preferences | Print Log'},
        component: PrintLogComponent,
      },
      {
        path: 'convert-log',
        data: {title: 'Preferences | Convert Log'},
        component: ConvertLogComponent,
      },
      {
        path: 'users',
        data: {title: 'Preferences | Users'},
        component: UsersComponent,
      }
    ]
  },
  {
    path: 'modal',
    data: {title: 'Modal'},
    component: ModalComponent
  }
];


@NgModule({
  imports: [RouterModule.forRoot(routes), CommonModule],
  exports: [RouterModule],
  declarations: []
})
export class AppRoutingModule {
}
