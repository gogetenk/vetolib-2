import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export default function LoginPage() {
  return (
    <Card data-testid="login-card">
      <CardHeader>
        <CardTitle>Welcome to Vetolib</CardTitle>
        <CardDescription>
          Sign in to manage your veterinary clinic
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="vet@clinic-dubai.com"
              data-testid="login-email-input"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              placeholder="Enter your password"
              data-testid="login-password-input"
            />
          </div>
          <Button
            type="submit"
            className="w-full"
            data-testid="login-submit-btn"
          >
            Sign In
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
